using KSP.Game;
using KSP.Game.Load;
using KSP.Logging;
using KSP.Sim;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using UnityEngine;

namespace KSP.Research {
    public class ResearchManager {
        public List < Technology > Technologies;

        private GameInstance _game => GameManager.Instance.Game;

        public Technology GetTechnologyByID(string ID) {
            Technology technologyById = (Technology) null;
            int index = this.Technologies.FindIndex((Predicate < Technology > )(x => x.ID == ID));
            if (index != -1)
                technologyById = this.Technologies[index];
            return technologyById;
        }

        public void Initialize() => this.LoadTechTreeFromAddressableData();

        public void UpdateTechTreeWithSaveGameData(LoadGameData gameData) {
            if (gameData == null)
                return;
            SerializedSavedGame savedGame = gameData.SavedGame;
        }

        public void ApplyTechTreeToSaveGame(LoadGameData gameData) {}

        private void LoadTechTreeFromAddressableData() {
            this.Technologies = new List < Technology > ();
            this._game.Assets.LoadByLabel < TextAsset > ("technology", (Action < TextAsset > ) null, (Action < IList < TextAsset >> )(allTechnologies => {
                foreach(TextAsset allTechnology in (IEnumerable < TextAsset > ) allTechnologies) {
                    try {
                        this.Technologies.Add(JsonConvert.DeserializeObject < Technology > (allTechnology.text));
                    } catch (Exception ex) {
                        GlobalLog.ErrorF(LogFilter.Gameplay, "Unable to deserialize technology from asset " + allTechnology.name + " : " + ex.Message);
                    }
                }
                foreach(Technology technology in this.Technologies)
                technology.Initialize();
            }));
        }

        public ref List < Technology > GetTechnologies() => ref this.Technologies;
    }
}