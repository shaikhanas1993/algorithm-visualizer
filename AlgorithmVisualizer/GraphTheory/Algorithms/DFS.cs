﻿using System;
using System.Collections.Generic;

using AlgorithmVisualizer.GraphTheory.Utils;
using static AlgorithmVisualizer.GraphTheory.FDGV.GraphVisualizer;

namespace AlgorithmVisualizer.GraphTheory.Algorithms
{
	class DFS : GraphAlgorithm
	{
		private int from, to;

		public DFS(Graph graph, int _from, int _to) : base(graph)
		{
			from = _from;
			to = _to;

			Solve();
		}

		public override void Solve()
		{
			// DFS to check connectivity - setup
			HashSet<int> visited = new HashSet<int>();
			bool reachedTo = Solve(from, visited);
			Console.WriteLine("There exists {0} path from {1} to {2}",
				reachedTo ? "a" : "no", from, to);
		}
		private bool Solve(int at, HashSet<int> visited)
		{
			// DFS to check connectivity
			visited.Add(at);
			graph.MarkParticle(at, at == to ? Colors.Red : Colors.Orange);
			Sleep(1500);
			if (at == to) return true;
			if (graph.AdjList[at] != null)
			{
				foreach (Edge edge in graph.AdjList[at])
				{
					// Does not visualize edges to already visited nodes to better show
					// backtracking
					if (!visited.Contains(edge.To))
					{
						// draw edge in orange to mark as visiting
						graph.MarkSpring(edge, Colors.Orange, 0);
						Sleep(1000);
						if (Solve(edge.To, visited))
						{
							// If the a path has been found to the end node(to) then
							// each node in the path will be highlighted here
							graph.MarkSpring(edge, Colors.Green, Dir.Directed);
							graph.MarkParticle(at, Colors.Green);
							Sleep(1500);
							return true;
						}
						// draw edge in dark gray to to mark as visited
						graph.MarkSpring(edge, Colors.Visited);
						Sleep(1000);
					}
				}
			}
			// Draw particle in darkGreyBrush (visited)
			graph.MarkParticle(at, Colors.Visited, Colors.VisitedBorder);
			Sleep(1000);
			return false;
		}
	}
}
