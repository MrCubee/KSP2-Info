using KSP.Game;
using KSP.Logging;
using System;
using System.Collections.Generic;
using UnityEngine;

namespace KSP.Research {
    public class Technology {
        public string ID;
        public string Name;
        public string IconAssetName;
        private Texture2D _icon;
        public bool IconLoadFinished;
        public TechnologyCategoryType TechnologyCategory;
        public string Description;
        public int ScienceCost;
        public List < Benefit > Benefits = new List < Benefit > ();
        public List < string > Prerequisites;
        public PrerequisiteLogicType PrerequisiteLogic;
        public TechnologyVisibilityType TechnologyVisibility;
        public Vector2 TechTreePosition;
        public List < Technology > Parents = new List < Technology > ();
        public List < Technology > Children = new List < Technology > ();

        private GameInstance _game => GameManager.Instance.Game;

        public Texture2D Icon => this._icon;

        public void Initialize() {
            if (!string.IsNullOrEmpty(this.IconAssetName))
                this._game.Assets.Load < Texture2D > (this.IconAssetName, (Action < Texture2D > )(result => {
                    if ((UnityEngine.Object) result == (UnityEngine.Object) null)
                        GlobalLog.ErrorF(LogFilter.Gameplay, "Error: unable to load icon for Technology " + this.ID);
                    this._icon = result;
                    this.IconLoadFinished = true;
                }), true);
            else
                this.IconLoadFinished = true;
            foreach(string prerequisite in this.Prerequisites) {
                Technology technologyById = this._game.ResearchManager.GetTechnologyByID(prerequisite);
                if (technologyById == null) {
                    GlobalLog.ErrorF(LogFilter.Gameplay, "Error: unable to find technology with ID " + prerequisite);
                } else {
                    technologyById.Children.Add(this);
                    this.Parents.Add(technologyById);
                }
            }
            this.UpdateVisibilityState();
        }

        public void UpdateVisibilityState() {
            if (this.TechnologyVisibility == TechnologyVisibilityType.Owned)
                return;
            this.Parents.RemoveAll((Predicate < Technology > )(x => x == null));
            if (this.Parents.Count == 0)
                this.TechnologyVisibility = TechnologyVisibilityType.Visible;
            else if (this.Parents.Count == 1) {
                if (this.Parents[0].TechnologyVisibility != TechnologyVisibilityType.Owned || this.TechnologyVisibility != TechnologyVisibilityType.Hidden)
                    return;
                this.TechnologyVisibility = TechnologyVisibilityType.Visible;
            } else if (this.PrerequisiteLogic == PrerequisiteLogicType.And) {
                bool flag = true;
                foreach(Technology parent in this.Parents) {
                    if (parent.TechnologyVisibility != TechnologyVisibilityType.Owned)
                        flag = false;
                }
                if (!flag)
                    return;
                this.TechnologyVisibility = TechnologyVisibilityType.Visible;
            } else {
                if (this.PrerequisiteLogic != PrerequisiteLogicType.Or)
                    return;
                foreach(Technology parent in this.Parents) {
                    if (parent.TechnologyVisibility == TechnologyVisibilityType.Owned)
                        this.TechnologyVisibility = TechnologyVisibilityType.Visible;
                }
            }
        }

        public void Unlock() {
            this.TechnologyVisibility = TechnologyVisibilityType.Owned;
            foreach(Benefit benefit in this.Benefits)
            benefit.Apply();
            foreach(Technology child in this.Children)
            child.UpdateVisibilityState();
        }
    }
}