using System.Collections.Generic;
using System.Linq;

namespace CarboLifeAPI
{
    /// <summary>
    /// Undo and redo over whole project snapshots (CarboProject.ToSnapshot).
    ///
    /// The history holds the state the project is in now, plus the states before it (undo) and
    /// after it (redo). Record is offered a snapshot whenever the project may have changed; one
    /// equal to the current state is ignored, so recalculating without editing adds no step.
    /// </summary>
    public class CarboUndoHistory
    {
        /// <summary>A large project is ~200 KB compressed, so this caps the history at a few MB.</summary>
        public const int MaxSteps = 30;

        private readonly LinkedList<byte[]> undo = new LinkedList<byte[]>();
        private readonly Stack<byte[]> redo = new Stack<byte[]>();
        private byte[] current;

        public bool CanUndo { get { return undo.Count > 0; } }
        public bool CanRedo { get { return redo.Count > 0; } }

        /// <summary>Forgets everything. The next Record becomes the starting state.</summary>
        public void Clear()
        {
            undo.Clear();
            redo.Clear();
            current = null;
        }

        /// <summary>Notes the project's state; true if it was an edit that became an undo step.</summary>
        public bool Record(byte[] snapshot)
        {
            if (current == null)
            {
                current = snapshot;
                return false;
            }

            if (current.SequenceEqual(snapshot))
                return false;

            undo.AddLast(current);
            if (undo.Count > MaxSteps)
                undo.RemoveFirst();

            current = snapshot;
            redo.Clear();
            return true;
        }

        /// <summary>The state to go back to, or null when there is none.</summary>
        public byte[] Undo()
        {
            if (undo.Count == 0)
                return null;

            redo.Push(current);
            current = undo.Last.Value;
            undo.RemoveLast();
            return current;
        }

        /// <summary>The state to go forward to, or null when there is none.</summary>
        public byte[] Redo()
        {
            if (redo.Count == 0)
                return null;

            undo.AddLast(current);
            current = redo.Pop();
            return current;
        }
    }
}
