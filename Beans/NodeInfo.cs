using ArcaeaUnlimitedAPI.Core;
using static ArcaeaUnlimitedAPI.Core.GlobalConfig;

namespace ArcaeaUnlimitedAPI.Beans;

internal static class NodeInfo
{
    private static int _nodeindex;

    private static Node GetNode(out int nodeIndex)
    {
        _nodeindex %= Config.Nodes.Count;
        nodeIndex = _nodeindex;
        return Config.Nodes[_nodeindex++];
    }

    internal static Node? Alloc()
    {
        var node = GetNode(out var nodeindex);
        while (!node.Active)
        {
            node = GetNode(out var curnodeindex);
            if (curnodeindex == nodeindex)
            {
                Config.Nodes.ForEach(Utils.TestNode);
                Log.FunctionError("Node", "ranout.");
                return null;
            }
        }

        return node;
    }
}
