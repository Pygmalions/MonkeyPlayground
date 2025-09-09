// For this example to work, the mdns.zip file needs to be extracted into this example folder. The
// mdns zip file contains two open source libraries that are used to implement the mdns functionality.
// This is only an example and not part of the rest server itself.
// The choice of the mDNS library is up to you, but it needs to be compatible with Unity.

// #define MDNS
#if MDNS
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using Makaretu.Dns;
using Makaretu.Dns.Resolving;
using RestServer;
using UnityEngine;
using UnityEngine.Networking;
using UnityEngine.UI;
using Random = System.Random;

namespace de.bearo.restserver.Samples.mDNSExample {
    [RequireComponent(typeof(RestServer.RestServer))]
    public class ExampleMDNSController : MonoBehaviour {
        public RestServer.RestServer restServer;
        public bool doAdvertisement = true;
        public Text textFoundServices;

        #region MDNS

        public const string ServiceNamePrefix = "unity-restserver-";
        private ServiceDiscovery _sdDiscover;
        private ServiceDiscovery _sdAdvertiser;
        private string _unityServiceName;

        #endregion

        #region Found Services

        public readonly ConcurrentSet<MyAddressRecord> PossibleAddresses = new();

        public readonly Dictionary<MyAddressRecord, bool> Candidates = new();

        public readonly Dictionary<MyAddressRecord, bool> FoundAddresses = new();

        #endregion

        public bool RequestInProgress { get; private set; }

        /// <summary>
        /// Advertisement of the unity-restservers, this is needed so they can be discovered.
        /// </summary>
        public void Advertise() {
            var rnd = new Random();

            _unityServiceName = ServiceNamePrefix + rnd.Next();
            var service = new ServiceProfile(_unityServiceName, "_http._tcp", 8080);
            _sdAdvertiser = new ServiceDiscovery();

            if (_sdAdvertiser.Probe(service)) {
                // service name already in use
                Debug.LogError($"Service name already in use: {service}");
            } else {
                Debug.Log($"Advertising service {service}");
                Debug.Log("!!! Note that advertising takes some time !!!");
                _sdAdvertiser.Advertise(service);
            }
        }

        /// <summary>
        /// Query for services and try to find unity-restserver instances.
        /// </summary>
        public void Discover() {
            _sdDiscover = new ServiceDiscovery();
            _sdDiscover.ServiceDiscovered += (s, serviceName) => {
                Debug.Log($"Service Discovered {serviceName}");
                Debug.Log($"Querying for service details {serviceName}");

                // Ask for the name of instances of the service.
                _sdDiscover.Mdns.SendQuery(serviceName, type: DnsType.PTR);
            };

            _sdDiscover.ServiceInstanceDiscovered += (s, e) => {
                Console.WriteLine($"XXX service instance discovered! '{e.ServiceInstanceName}'");

                // Ask for the service instance details.
                _sdDiscover.Mdns.SendQuery(e.ServiceInstanceName, type: DnsType.SRV);
            };

            _sdDiscover.Mdns.AnswerReceived += (s, e) => {
                // Is this an answer to a service instance details?
                var servers = e.Message.Answers.OfType<SRVRecord>();
                foreach (var server in servers) {
                    // query everything to find our server

                    Debug.Log($"XXX server '{server.Target}' for '{server.Name}' - {server}");

                    // Ask for the host IP addresses.
                    _sdDiscover.Mdns.SendQuery(server.Target, type: DnsType.A);
                    _sdDiscover.Mdns.SendQuery(server.Target, type: DnsType.AAAA);
                }

                // Is this an answer to host addresses?
                var addresses = e.Message.Answers.OfType<AddressRecord>();
                foreach (var address in addresses) {
                    // only listen for our own addresses

                    Debug.Log($"XXX address '{address.Name}' at {address.Address} - {address}");

                    if (address.Name.ToString().StartsWith(ServiceNamePrefix)) {
                        // the server is configured to listen to all ipv4 - so no need to check ipv6
                        if (address.Address.AddressFamily == AddressFamily.InterNetwork) {
                            PossibleAddresses.Add(new MyAddressRecord(
                                address.Address,
                                "http://" + address.Address + ":8080",
                                address.Name.ToString()
                            ));
                        }
                    }
                }
            };
        }

        public void Start() {
            if (restServer == null) {
                restServer = GetComponent<RestServer.RestServer>();
            }

            restServer.EndpointCollection.RegisterEndpoint(HttpMethod.GET, "/", request => { request.CreateResponse().Status(200).SendAsync(); });

            Discover();

            if (doAdvertisement) {
                Advertise();
            }
        }

        public void OnDestroy() {
            Stop();
        }

        public void OnDisable() {
            Stop();
        }

        public void Stop() {
            _sdAdvertiser?.Dispose();
            _sdDiscover?.Dispose();
        }

        void Update() {
            if (PossibleAddresses.Count == 0) {
                return;
            }

            // query found ip addresses if they are reachable from this network. 
            foreach (var possibleAddress in PossibleAddresses) {
                if (!Candidates.ContainsKey(possibleAddress)) {
                    Candidates.Add(possibleAddress, false);
                }
            }

            foreach (var candidate in Candidates) {
                if (candidate.Value) {
                    continue;
                }

                RequestInProgress = true;
                StartCoroutine(GetRequest(candidate.Key));
            }

            if (FoundAddresses.Count > 0) {
                textFoundServices.text =
                    "Advertisements have started; no service has been discovered so far. Note that discovery heavily depends on network " +
                    "support and needs some time for the services to be discovered. \nDiscovered unity rest servers:\n\n" +
                    FoundAddresses.Where(c => c.Value).Select(c => "* " + c.Key + "\n").Aggregate((s, s1) => s + s1);
            }
        }

        IEnumerator GetRequest(MyAddressRecord address) {
            Debug.Log("Requesting " + address + " to see if the rest server is reachable...");
            var uwr = new UnityWebRequest(address.Url);
            uwr.timeout = 1; // s ! :(
            yield return uwr.SendWebRequest();
            RequestInProgress = false;

            switch (uwr.result) {
                case UnityWebRequest.Result.ConnectionError:
                case UnityWebRequest.Result.DataProcessingError:
                case UnityWebRequest.Result.ProtocolError:
                    Candidates[address] = true;
                    Debug.LogError("No connection to " + address);
                    break;
                case UnityWebRequest.Result.Success:
                    Candidates[address] = true;
                    if (!FoundAddresses.ContainsKey(address)) {
                        FoundAddresses.Add(address, true);
                    }

                    break;
            }
        }

        public class MyAddressRecord : IEquatable<MyAddressRecord> {
            public readonly IPAddress IPAddress;
            public readonly string Url;
            public readonly string Name;

            public MyAddressRecord(IPAddress ipAddress, string url, string name) {
                IPAddress = ipAddress;
                Url = url;
                Name = name;
            }

            public bool Equals(MyAddressRecord other) {
                return Url == other.Url;
            }

            public override bool Equals(object obj) {
                if (obj is null) return false;
                if (ReferenceEquals(this, obj)) return true;
                if (obj.GetType() != GetType()) return false;
                return Equals((MyAddressRecord)obj);
            }

            public override int GetHashCode() {
                return (Url != null ? Url.GetHashCode() : 0);
            }

            public override string ToString() {
                return $"{Url} - {Name}";
            }
        }
    }
}
#endif