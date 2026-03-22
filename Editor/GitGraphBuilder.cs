using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace GitIntegration
{
    // Data yielded per commit row

    public class GraphRow
    {
        public GitCommitInfo Commit;

        /// <summary>Which column the commit dot sits on.</summary>
        public int Lane;

        /// <summary>Max lane index in this row (for sizing).</summary>
        public int MaxLane;

        /// <summary>Colour of the commit dot.</summary>
        public Color DotColor;

        /// <summary>Lanes passing through without touching the commit.</summary>
        public List<(int lane, Color color)> Passthrough = new List<(int, Color)>();

        /// <summary>Edges converging into the commit from above.</summary>
        public List<(int fromLane, Color color)> Converging = new List<(int, Color)>();

        /// <summary>Edges diverging from the commit downward.</summary>
        public List<(int toLane, Color color)> Diverging = new List<(int, Color)>();
    }

    // Graph builder – assigns lanes and builds drawing data

    public static class GitGraphBuilder
    {
        private static readonly Color[] PALETTE =
        {
            new Color(0.38f, 0.62f, 1.00f),   // blue
            new Color(0.35f, 0.85f, 0.45f),   // green
            new Color(0.95f, 0.55f, 0.25f),   // orange
            new Color(0.80f, 0.35f, 0.90f),   // purple
            new Color(0.25f, 0.85f, 0.85f),   // cyan
            new Color(0.95f, 0.82f, 0.28f),   // yellow
            new Color(0.95f, 0.35f, 0.62f),   // pink
            new Color(0.60f, 0.88f, 0.32f),   // lime
        };

        private static int _nextColor;
        private static Color PickColor() => PALETTE[(_nextColor++) % PALETTE.Length];

        // Internal lane slot

        private struct LaneSlot
        {
            public string TargetHash;   // hash of commit this lane is heading toward
            public Color  LaneColor;
            public bool   Active => TargetHash != null;
        }

        public static List<GraphRow> Build(List<GitCommitInfo> commits)
        {
            _nextColor = 0;
            var lanes  = new List<LaneSlot>();
            var result = new List<GraphRow>(commits.Count);

            foreach (var commit in commits)
            {
                var parents = commit.Parents ?? new List<string>();

                // 1. Find or create this commit's lane

                int myLane   = FindLane(lanes, commit.Hash);
                bool wasNew  = myLane < 0;

                if (wasNew)
                {
                    myLane = FindFreeLane(lanes);
                    if (myLane < 0) myLane = lanes.Count;
                    EnsureCapacity(lanes, myLane);
                }

                Color myColor = wasNew ? PickColor() : lanes[myLane].LaneColor;

                // 2. Snapshot before-state

                EnsureCapacity(lanes, myLane);
                if (wasNew) lanes[myLane] = new LaneSlot { TargetHash = commit.Hash, LaneColor = myColor };

                var before = lanes.ToArray();

                // 3. Extra converging lanes

                var extraConverge = new List<int>();
                for (int i = 0; i < before.Length; i++)
                {
                    if (i != myLane && before[i].Active && before[i].TargetHash == commit.Hash)
                        extraConverge.Add(i);
                }

                // Free those lanes
                foreach (int ec in extraConverge)
                    lanes[ec] = default;

                // 4. Update myLane for parent[0]

                int diverge0Lane = -1;

                if (parents.Count == 0)
                {
                    lanes[myLane] = default;  // initial commit, lane ends
                }
                else
                {
                    int fp0 = FindLane(lanes, parents[0]);
                    if (fp0 >= 0 && fp0 != myLane)
                    {
                        // Parent[0] already tracked by another lane - merge
                        lanes[myLane] = default;
                        diverge0Lane  = fp0;
                    }
                    else
                    {
                        lanes[myLane] = new LaneSlot { TargetHash = parents[0], LaneColor = myColor };
                        diverge0Lane  = myLane;
                    }
                }

                // 5. Add new lanes for extra parents

                var extraDiverge = new List<(int toLane, Color color)>();
                for (int pi = 1; pi < parents.Count; pi++)
                {
                    if (FindLane(lanes, parents[pi]) >= 0) continue;

                    int slot  = FindFreeLane(lanes);
                    if (slot < 0) slot = lanes.Count;
                    EnsureCapacity(lanes, slot);

                    Color ec = PickColor();
                    lanes[slot] = new LaneSlot { TargetHash = parents[pi], LaneColor = ec };
                    extraDiverge.Add((slot, ec));
                }

                // 6. Build row

                var row = new GraphRow { Commit = commit, Lane = myLane, DotColor = myColor };

                // Passthrough: active both before and after, not the commit lane
                for (int i = 0; i < before.Length; i++)
                {
                    if (i == myLane) continue;
                    if (!before[i].Active) continue;
                    if (before[i].TargetHash == commit.Hash) continue;
                    if (i < lanes.Count && lanes[i].Active && lanes[i].TargetHash == before[i].TargetHash)
                        row.Passthrough.Add((i, before[i].LaneColor));
                }

                // Converging (top-half)
                if (!wasNew)
                    row.Converging.Add((myLane, myColor));
                foreach (int ec in extraConverge)
                    row.Converging.Add((ec, before[ec].LaneColor));

                // Diverging (bottom-half)
                if (diverge0Lane >= 0)
                    row.Diverging.Add((diverge0Lane, myColor));
                foreach (var (tl, tc) in extraDiverge)
                    row.Diverging.Add((tl, tc));

                // MaxLane
                int maxIdx = myLane;
                foreach (var (l, _) in row.Passthrough) maxIdx = Math.Max(maxIdx, l);
                foreach (var (fl, _) in row.Converging)  maxIdx = Math.Max(maxIdx, fl);
                foreach (var (tl, _) in row.Diverging)   maxIdx = Math.Max(maxIdx, tl);
                row.MaxLane = maxIdx + 1;

                result.Add(row);

                // 7. Trim trailing free lanes

                while (lanes.Count > 0 && !lanes[lanes.Count - 1].Active)
                    lanes.RemoveAt(lanes.Count - 1);
            }

            return result;
        }

        // Helpers

        private static int FindLane(List<LaneSlot> lanes, string hash)
        {
            for (int i = 0; i < lanes.Count; i++)
                if (lanes[i].Active && lanes[i].TargetHash == hash) return i;
            return -1;
        }

        private static int FindFreeLane(List<LaneSlot> lanes)
        {
            for (int i = 0; i < lanes.Count; i++)
                if (!lanes[i].Active) return i;
            return -1;
        }

        private static void EnsureCapacity(List<LaneSlot> lanes, int index)
        {
            while (lanes.Count <= index) lanes.Add(default);
        }
    }
}
