using Blish_HUD.Controls;
using System.Collections.Generic;
using System;
using Microsoft.Xna.Framework;
using System.Linq;

namespace FarmingTracker
{
    public class SortPanel : Panel
    {
        public SortPanel(Container parent, SortByWithDirection sortByWithDirection)
        {
            BackgroundColor = Color.Black * 0.5f;
            Height = 38;
            Parent = parent;

            _label = new Label
            {
                AutoSizeHeight = true,
                AutoSizeWidth = true,
                Top = 10,
                Left = 10,
                Parent = this,
            };

            Dropdown = new Dropdown
            {
                BackgroundColor = Color.Black * 0.5f,
                Width = 300,
                Top = 5,
                Left = 70,
                Parent = this
            };

            var dropDownTextDict = GetSortByDropDownTexts();

            foreach (string dropDownText in dropDownTextDict.Values)
                Dropdown.Items.Add(dropDownText);

            Dropdown.SelectedItem = dropDownTextDict[sortByWithDirection];

            RemoveSortButton = new StandardButton
            {
                Text = "x",
                Width = 28,
                Top = 5,
                Left = Dropdown.Right + 5,
                Parent = this
            };

            Width = RemoveSortButton.Right + 5;
        }

        public SortByWithDirection GetSelectedSortBy()
        {
            return GetSortByDropDownTexts().First(d => d.Value == Dropdown.SelectedItem).Key;
        }

        public StandardButton RemoveSortButton { get; }
        public Dropdown Dropdown { get; }

        public void SetLabelText(string text)
        {
            _label.Text = text;
        }

        private static Dictionary<SortByWithDirection, string> GetSortByDropDownTexts()
        {
            var allSortBy = (SortByWithDirection[])Enum.GetValues(typeof(SortByWithDirection));
            var dropDownTextBySortBy = new Dictionary<SortByWithDirection, string>();

            foreach (var sortBy in allSortBy)
                dropDownTextBySortBy[sortBy] = Helper.ConvertEnumValueToTextWithBlanks(sortBy.ToString());

            return dropDownTextBySortBy;
        }

        private readonly Label _label;
    }
}
