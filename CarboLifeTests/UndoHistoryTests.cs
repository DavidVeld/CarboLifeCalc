using CarboLifeAPI;
using CarboLifeAPI.Data;
using System.IO;
using System.Linq;
using Xunit;

namespace CarboLifeTests
{
    /// <summary>
    /// Undo in the main window swaps in a project read back from a snapshot. These pin the two
    /// things that rests on: a snapshot reads back to the same project, and the history hands
    /// back the right snapshot.
    /// </summary>
    public class UndoHistoryTests
    {
        private static CarboProject LoadSample()
        {
            string path = Path.Combine(PathUtils.GetAssemblyDir(), "samples", "CarboLifeCalc_sample_project_R23.clcx");
            CarboProject project = new CarboProject().DeSerializeXML(path);
            Assert.NotNull(project);

            project.CalculateProject();
            return project;
        }

        private static CarboProject Restore(byte[] snapshot)
        {
            CarboProject project = CarboProject.FromSnapshot(snapshot);
            project.CalculateProject();
            return project;
        }

        /// <summary>
        /// Byte for byte, not just the totals: after an undo the pages recalculate, and if that
        /// produced a different snapshot it would count as a new edit and wipe the redo steps.
        /// </summary>
        [Fact]
        public void Snapshot_reads_back_to_the_same_project()
        {
            CarboProject project = LoadSample();
            byte[] before = project.ToSnapshot();

            CarboProject restored = Restore(before);

            Assert.Equal(project.ECTotal, restored.ECTotal);
            Assert.Equal(project.getGroupList.Count, restored.getGroupList.Count);
            Assert.True(before.SequenceEqual(restored.ToSnapshot()), "Restored project does not snapshot the same");
        }

        [Fact]
        public void Saving_is_not_an_edit()
        {
            CarboProject project = LoadSample();
            byte[] before = project.ToSnapshot();

            project.justSaved = true;
            project.filePath = @"C:\elsewhere\renamed.clcx";

            Assert.True(before.SequenceEqual(project.ToSnapshot()));
            Assert.Equal(@"C:\elsewhere\renamed.clcx", project.filePath);
            Assert.True(project.justSaved);
        }

        [Fact]
        public void Undo_and_redo_move_between_edits()
        {
            CarboProject project = LoadSample();
            double original = project.ECTotal;

            CarboUndoHistory history = new CarboUndoHistory();
            Assert.False(history.Record(project.ToSnapshot()));
            Assert.False(history.Record(project.ToSnapshot()));
            Assert.False(history.CanUndo);

            project.DeleteGroup(project.getGroupList[0]);
            project.CalculateProject();
            double edited = project.ECTotal;
            Assert.NotEqual(original, edited);
            Assert.True(history.Record(project.ToSnapshot()));

            CarboProject undone = Restore(history.Undo());
            Assert.Equal(original, undone.ECTotal);
            Assert.Null(history.Undo());

            CarboProject redone = Restore(history.Redo());
            Assert.Equal(edited, redone.ECTotal);
            Assert.Null(history.Redo());

            //A new edit after an undo drops the redo steps.
            history.Undo();
            Assert.True(history.CanRedo);
            undone.UncertFact += 0.05;
            undone.CalculateProject();
            Assert.True(history.Record(undone.ToSnapshot()));
            Assert.False(history.CanRedo);
        }
    }
}
