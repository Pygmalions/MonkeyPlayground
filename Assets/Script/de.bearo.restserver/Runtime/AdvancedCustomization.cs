using System;
using System.Net;
using System.Net.Sockets;
using RestServer.WebSocket;
using UnityEngine;

namespace RestServer {
    public class AdvancedCustomization {
        public virtual LowLevelServer CreateLowLevelServer(
            EndpointCollection endpointCollection, WsSessionCollection wsSessionCollection, IPAddress listenAddress, int port,
            SpecialHandlers specialHandlers, bool debugLog, Action<Socket> additionalSocketConfigurationServer,
            Action<Socket> additionalSocketConfigurationSession
        ) {
            return new LowLevelServer(
                endpointCollection, wsSessionCollection, listenAddress, port, specialHandlers, debugLog,
                additionalSocketConfigurationServer, additionalSocketConfigurationSession, this
            );
        }

        public virtual LowLevelSession CreateSession(LowLevelServer lowLevelServer, Logger logger, SpecialHandlers specialHandlers, bool debugLog) {
            logger.Log("CreateSession for new incoming request");
            return new LowLevelSession(lowLevelServer, specialHandlers, debugLog);
        }

        public virtual void OnWsConnect(WSEndpointId endpointId, LowLevelSession session) { }

        public virtual void OnWsDisconnect(WSEndpointId endpointId, LowLevelSession session) { }
    }
}