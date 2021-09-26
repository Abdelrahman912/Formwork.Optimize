using FormworkOptimize.Core.Enums;

namespace FormworkOptimize.App.Models
{
    public class SystemProductivityModel
    {
       
        #region Properties

        /// <summary>
        /// Productivit of Man Power (m^2/day)
        /// </summary>
        public double Productivity { get; set; }

        public FormworkSystem System { get;  }

        #endregion

        #region Constructors

        public SystemProductivityModel(double productivity, FormworkSystem system)
        {
            Productivity = productivity;
            System = system;
        }

        #endregion

    }
}
