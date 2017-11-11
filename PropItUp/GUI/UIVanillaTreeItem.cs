using ColossalFramework.UI;
using UnityEngine;
using static PropItUp.Configuration;

namespace PropItUp.GUI
{
    public class UIVanillaTreeItem : UIPanel, IUIFastListRow
    {
        private UILabel _name;
        private TreeInfo _tree;
        public TreeInfo tree;

        protected override void OnSizeChanged()
        {
            base.OnSizeChanged();

            if (_name == null) return;
        }

        private void SetupControls()
        {
            if (_name != null) return;

            isVisible = true;
            isInteractive = true;
            width = parent.width;
            height = UIUtils.c_fastListRowHeight;

            _name = AddUIComponent<UILabel>();
            _name.name = "TreeName";
            _name.relativePosition = new Vector3(5, 9);
            _name.textColor = new Color32(238, 238, 238, 255);
            _name.textScale = 0.85f;
        }

        protected override void OnMouseDown(UIMouseEventParameter p)
        {
            p.Use();

            base.OnMouseDown(p);
        }

        protected override void OnMouseEnter(UIMouseEventParameter p)
        {
            base.OnMouseEnter(p);
        }

        protected override void OnMouseLeave(UIMouseEventParameter p)
        {
            base.OnMouseLeave(p);
        }

        #region IUIFastListRow implementation
        public void Display(object data, bool isRowOdd)
        {
            SetupControls();

            _tree = data as TreeInfo;

            PrefabReplacement replacement = (TreeReplacerPanel.instance.isVisible) ? PropItUpTool.config.GetGlobalReplacementByTreeName(_tree.name) : PropItUpTool.config.GetGlobalBuildingReplacementByTreeName(_tree.name);

            if (replacement == null)
            {
                _name.text = UIUtils.GenerateBeautifiedPrefabName(_tree);
            }
            else
            {
                TreeInfo replacementTree = PrefabCollection<TreeInfo>.FindLoaded(replacement.replacement_name);
                _name.text = $"{UIUtils.GenerateBeautifiedPrefabName(_tree)}  [original: {UIUtils.GenerateBeautifiedPrefabName(replacementTree)}]";
            }

            backgroundSprite = null;
        }

        public void Select(bool isRowOdd)
        {
            backgroundSprite = "ListItemHighlight";
            color = new Color32(255, 255, 255, 255);
        }

        public void Deselect(bool isRowOdd)
        {
            backgroundSprite = null;
        }
        #endregion
    }
}