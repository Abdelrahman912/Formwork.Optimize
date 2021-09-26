namespace FormworkOptimize.Core.Entities.Cost
{
    public class FormworkTimeLine
    {
        #region Properties

        public int InstallationDuration { get; }

        public int SmitheryDuration { get; }

        public int WaitingDuaration { get; }

        public int RemovalDuration { get;}

        public int TotalDuration =>
            InstallationDuration + SmitheryDuration + WaitingDuaration + RemovalDuration;

        #endregion

        #region Constructors

        public FormworkTimeLine(int installationDuration, int smitheryDuration, int waitingDuaration, int removalDuration)
        {
            InstallationDuration = installationDuration;
            SmitheryDuration = smitheryDuration;
            WaitingDuaration = waitingDuaration;
            RemovalDuration = removalDuration;
        }

        #endregion

    }
}
