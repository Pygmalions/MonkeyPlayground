#if RESTSERVER_VISUALSCRIPTING
using RestServer.Runtime.VisualScripting.Nodes;
using Unity.VisualScripting;

namespace RestServer.Editor.VisualScripting.Nodes {
    [Descriptor(typeof(GetBodyNode))]
    public class GetBodyNodeDescriptor : UnitDescriptor<GetBodyNode> {
        public GetBodyNodeDescriptor(GetBodyNode target) : base(target) { }

        protected override void DefinedPort(IUnitPort port, UnitPortDescription description) {
            base.DefinedPort(port, description);

            switch (port.key) {
                case nameof(GetBodyNode.outputBody):
                    description.label = "Body";
                    description.summary = "Body from the request.";
                    break;
            }
        }
    }
}
#endif