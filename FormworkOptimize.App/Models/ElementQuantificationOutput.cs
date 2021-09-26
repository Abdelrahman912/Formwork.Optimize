using CsvHelper.Configuration.Attributes;

namespace FormworkOptimize.App.Models
{
    public class ElementQuantificationOutput
    {
        #region Properties

        [Name("Item Name")]
        public string ItemName { get; }

        public int Quantity { get; }

        #endregion

        #region Constructors

        public ElementQuantificationOutput(string itemName , int count)
        {
            ItemName = itemName;
            Quantity = count;
        }

        #endregion

    }
}
