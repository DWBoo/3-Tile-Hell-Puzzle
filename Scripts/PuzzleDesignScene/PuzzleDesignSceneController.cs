using Assets.Scripts.Extensions;
using Assets.Scripts.Internal;
using Assets.Scripts.Scene.PuzzleScene;
using DataContainer.Generated;
using Dignus.Log;
using Dignus.Unity;
using Dignus.Unity.Framework;
using GameContents;
using System.Collections.Generic;
using System.Linq;
using TemplateContainers;
using UnityEngine;
using UnityEngine.Events;

namespace Assets.Scripts.Scene.PuzzleDesignScene
{
    public class PuzzleDesignSceneController : SceneController<PuzzleDesignScene>
    {
        public readonly List<Color> TileColorList = new List<Color>
        {
            new Color(1f, 0.2f, 0f),
            new Color(1f, 0.4f, 0f),
            new Color(1f, 0.6f, 0f),
            new Color(1f, 0.8f, 0f),
            new Color(1f, 1f, 0f),
            new Color(1f, 0f, 0.2f),
            new Color(1f, 0f, 0.4f),
            new Color(1f, 0f, 0.6f),
            new Color(1f, 0f, 0.8f),
            new Color(1f, 0f, 1f),
            new Color(0.5f, 0.2f, 0.2f),
            new Color(0.5f, 0.4f, 0.4f),
            new Color(0.5f, 0.6f, 0.6f),
            new Color(0.5f, 0.8f, 0.8f),
            new Color(0.5f, 1f, 1f),
            new Color(0.2f, 0.3f, 0.5f),
            new Color(0.4f, 0.5f, 0.5f),
            new Color(0.6f, 0.7f, 0.5f),
            new Color(0.8f, 0.9f, 0.5f),
            new Color(0.2f, 0.2f, 0.2f),
            new Color(0.5f, 0.5f, 0.5f),
            new Color(0.8f, 0.8f, 0.8f),
            new Color(1f, 1f, 0f),
        };

        public const int MaxPuzzleX = 7;
        public const int MaxPuzzleY = 9;

        private int _defaultCount = -1;
        private Vector3 lastOverlapTilePos;
        private Vector3 lastTilePos;

        public void Load(string boardName)
        {
            if (Scene.SceneModel.PuzzleBoard == null)
            {
                var path = $"{GetSavePath()}/{boardName}";
                Scene.SceneModel.PuzzleBoard = Scene.InstantiateWithPool<PuzzleBoardObject>(path);

                if (Scene.SceneModel.PuzzleBoard == null)
                {
                    LogHelper.Info("존재하지 않는 보드명입니다.");
                    return;
                }

                foreach (var layer in Scene.SceneModel.PuzzleBoard.GetDesignPuzzleLayer())
                {
                    Scene.SceneModel.Layers.Add(layer);
                    Scene.SceneModel.TilesDic.Add(layer, layer.GetPuzzleTileObjects().ToList());
                    Scene.SceneModel.CurrentPuzzleLayer = layer;

                    string layerName = $"Layer_{Scene.SceneModel.LayerNameList.Count + 1}";
                    Scene.SceneModel.TileTotalCount += layer.GetPuzzleTileObjects().Count;
                    Scene.SceneModel.LayerNameList.Add(layerName);
                    Scene.SceneModel.TileCountDic.Add(layerName, layer.GetPuzzleTileObjects().Count);
                    Scene.SceneModel.LayerIndex = Scene.SceneModel.LayerNameList.Count - 1;
                    Scene.SceneModel.AddBoardPosition(layer.BoardPosition);

                    var sceneBoardConfig = Scene.GetBoardConfiguration();

                    var loadBoardConfig = Scene.SceneModel.PuzzleBoard.GetBoardConfiguration();

                    sceneBoardConfig.Clear();
                    //얕은 복사가 되는 것을 방지하기 위해 돌면서 데이터를 넣어준다.
                    foreach (var item in loadBoardConfig.GetTileDefinitions())
                    {
                        sceneBoardConfig.AddTileDefinition(item.Name, item.Count);
                    }
                }
            }
        }

        public void Save(UnityAction<GameObject> onSaving)
        {
            if (Scene.SceneModel.CurrentPuzzleLayer == null)
            {
                return;
            }

            if (Scene.SceneModel.TileTotalCount % 3 != 0)
            {
                LogHelper.Info("타일의 총 개수가 3의 배수가 되어야합니다.");
                return;
            }

            if (Scene.SceneModel.TileTotalCount < 9)
            {
                LogHelper.Info("타일이 최소 9개는 배치되어야 합니다.");
                return;
            }
            var config = Scene.GetBoardConfiguration();
            var tileDefinitionCount = 0;
            var usedTileName = new HashSet<string>();
            foreach (var tileDefinition in config.GetTileDefinitions())
            {
                //네임으로 데이터를 찾는다.
                var template = TemplateContainer<TileTemplate>.Find(tileDefinition.Name);
                //데이터가 옳바르지 않을시에 TemplateId가 -1로 넘어온다.
                //Invalid 메소드로 그냥 체크하면 된다.
                if (template.Invalid())
                {
                    LogHelper.Error($"정의되지 않은 타일 값이 저장되어 있습니다. {tileDefinition.Name}");
                    return;
                }

                if (tileDefinition.Count > 0)
                {
                    if (tileDefinition.Count % 3 != 0)
                    {
                        LogHelper.Error($"설정 값이 3의 배수가 아닙니다. {tileDefinition.Count}");
                        return;
                    }

                    tileDefinitionCount += tileDefinition.Count;
                }

                if (usedTileName.Contains(tileDefinition.Name) == true)
                {
                    LogHelper.Error($"이미 세팅한 타일이 저장되어 있습니다. {tileDefinition.Name}");
                    return;
                }

                usedTileName.Add(tileDefinition.Name);
            }

            if (tileDefinitionCount > Scene.SceneModel.TileTotalCount)
            {
                LogHelper.Error($"타일의 총합보다 풀 속성의 타일의 총합이 더 큽니다. {tileDefinitionCount} > {Scene.SceneModel.TileTotalCount}");
                return;
            }
            Scene.SceneModel.PuzzleBoard.SetBoardConfiguration(config);

            SetActiveAllLayer(true);
            onSaving?.Invoke(Scene.SceneModel.PuzzleBoard.gameObject);
            LogHelper.Debug($"보드가 정상적으로 저장되었습니다.");

            ReloadConfiguration();
        }

        public void EnablePuzzleSetting(float x, float y)
        {
            if (Scene.SceneModel.CurrentPuzzleLayer == null)
            {
                return;
            }

            if (CheckThreshold(x, y))
            {
                int castedPosX = (int)(x - Scene.GetMovingPosition().x);
                int castedPosY = (int)(y + Scene.GetMovingPosition().y);

                PuzzleTileObject tile;
                if (Scene.SceneModel.TryRemovingTile(AdjustTilePos(castedPosX, castedPosY),
                    Scene.SceneModel.GetBoardPosition(), out tile))
                {
                    if (!tile.IsNull())
                    {
                        Scene.SceneModel.CurrentPuzzleLayer.RemoveDesignPuzzleTile(tile);
                        Scene.SceneModel.TileTotalCount--;
                        Scene.SceneModel.TileCountDic[Scene.SceneModel.GetCurrentLayerName()]--;
                    }

                    return;
                }

                tile = Scene.SceneModel.CurrentPuzzleLayer.InstantiateWithPool<PuzzleTileObject>();
                lastTilePos = AdjustTilePos(castedPosX, castedPosY);
                SetTile(lastTilePos, tile, false);
                //디자인 툴에서 관리 및 사용을 위한 타일 저장.
                Scene.SceneModel.TilesDic[Scene.SceneModel.CurrentPuzzleLayer].Add(tile);
                //실제 로직에서 사용을 위한 타일 저장.
                Scene.SceneModel.CurrentPuzzleLayer.AddPuzzleTile(tile);
                Scene.SceneModel.TileCountDic[Scene.SceneModel.GetCurrentLayerName()]++;
                Scene.SceneModel.TileTotalCount++;
            }
        }

        public void EnableOverlapPuzzleSetting(string direction)
        {
            PuzzleTileObject tile = Scene.SceneModel.CurrentPuzzleLayer.InstantiateWithPool<PuzzleTileObject>();

            switch (direction)
            {
                case "Down":
                    lastOverlapTilePos.y -= 0.15f;
                    break;
                case "Up":
                    lastOverlapTilePos.y += 0.15f;
                    break;
                case "Left":
                    lastOverlapTilePos.x -= 0.15f;
                    break;
                case "Right":
                    lastOverlapTilePos.x += 0.15f;
                    break;
                case "Left&Down":
                    lastOverlapTilePos.x -= 0.15f;
                    lastOverlapTilePos.y -= 0.15f;
                    break;
                case "Left&Up":
                    lastOverlapTilePos.x -= 0.15f;
                    lastOverlapTilePos.y += 0.15f;
                    break;
                case "Right&Down":
                    lastOverlapTilePos.x += 0.15f;
                    lastOverlapTilePos.y -= 0.15f;
                    break;
                case "Right&Up":
                    lastOverlapTilePos.x += 0.15f;
                    lastOverlapTilePos.y += 0.15f;
                    break;
            }

            SetTile(lastTilePos, tile, true);
            tile.transform.position = lastOverlapTilePos;
            //디자인 툴에서 관리 및 사용을 위한 타일 저장.
            Scene.SceneModel.TilesDic[Scene.SceneModel.CurrentPuzzleLayer].Add(tile);
            //실제 로직에서 사용을 위한 타일 저장.
            Scene.SceneModel.CurrentPuzzleLayer.AddPuzzleTile(tile);
            Scene.SceneModel.TileCountDic[Scene.SceneModel.GetCurrentLayerName()]++;
            Scene.SceneModel.TileTotalCount++;
        }

        public void AddPuzzleLayer()
        {
            if (Scene.SceneModel.PuzzleBoard == null)
            {
                var board = Scene.InstantiateWithPool<PuzzleBoardObject>();
                Scene.SceneModel.PuzzleBoard = board;
            }

            var layer = Scene.SceneModel.PuzzleBoard.InstantiateWithPool<PuzzleLayerObject>();
            layer.transform.position = new Vector3(
                0f, 0f, layer.transform.position.z - Scene.SceneModel.TilesDic.Keys.Count);
            //디자인 툴에서 관리 및 사용을 위한 레이어 저장.
            Scene.SceneModel.Layers.Add(layer);
            //실제 로직에서 사용을 위한 레이어 저장.
            Scene.SceneModel.PuzzleBoard.AddDesignPuzzleLayerGo(layer);
            Scene.SceneModel.CurrentPuzzleLayer = layer;
            Scene.SceneModel.TilesDic.Add(layer, new List<PuzzleTileObject>());

            string layerName = $"Layer_{Scene.SceneModel.LayerNameList.Count + 1}";
            Scene.SceneModel.LayerNameList.Add(layerName);
            Scene.SceneModel.TileCountDic.Add(layerName, 0);
            Scene.SceneModel.LayerIndex = Scene.SceneModel.LayerNameList.Count - 1;
            Scene.SceneModel.AddBoardPosition(BoardPosition.None);
        }

        private void SetTile(Vector3 tilePos, PuzzleTileObject tile, bool isOverlap)
        {
            tile.GridTilePos = tilePos;
            LogHelper.Debug(tile.GridTilePos.ToString());
            switch (Scene.SceneModel.GetBoardPosition())
            {
                case BoardPosition.Up:
                    tile.GridTilePos -= new Vector3(0f, -0.5f, 0f);
                    break;

                case BoardPosition.Down:
                    tile.GridTilePos -= new Vector3(0f, 0.5f, 0f);
                    break;

                case BoardPosition.Left:
                    tile.GridTilePos += new Vector3(0.5f, 0f, 0f);
                    break;

                case BoardPosition.Right:
                    tile.GridTilePos += new Vector3(-0.5f, 0f, 0f);
                    break;

                case BoardPosition.DiagonalUpRight:
                    tile.GridTilePos = new Vector3(tile.GridTilePos.x + (-0.5f), tile.GridTilePos.y - (-0.5f), 0f);
                    break;

                case BoardPosition.DiagonalUpLeft:
                    tile.GridTilePos = new Vector3(tile.GridTilePos.x + 0.5f, tile.GridTilePos.y - (-0.5f), 0f);
                    break;

                case BoardPosition.DiagonalDownLeft:
                    tile.GridTilePos = new Vector3(tile.GridTilePos.x + 0.5f, tile.GridTilePos.y - 0.5f, 0f);
                    break;

                case BoardPosition.DiagonalDownRight:
                    tile.GridTilePos = new Vector3(tile.GridTilePos.x + (-0.5f), tile.GridTilePos.y - 0.5f, 0f);
                    break;

                default:
                    break;
            }

            tile.GetSpriteRenderer().color = TileColorList[Scene.SceneModel.LayerIndex];

            if (isOverlap)
            {
                tile.transform.position = lastOverlapTilePos;
            }
            else
            {
                tile.transform.position = new Vector3(tilePos.x + 0.5f, MaxPuzzleY - tilePos.y - 0.5f);
            }

            lastOverlapTilePos = tile.transform.position;
        }

        public void SetActiveAllLayer(bool isActive)
        {
            Scene.SceneModel.Layers.ForEach(layer => layer.gameObject.SetActive(isActive));
        }

        public Vector3 AdjustTilePos(int x, int y)
        {
            Vector3 movingPosition = Scene.GetMovingPosition();
            return new Vector3(x + movingPosition.x, y - movingPosition.y, 0f);
        }

        public bool CheckThreshold(float x, float y)
        {
            Vector3 movingPosition = Scene.GetMovingPosition();
            if (x > 0 + movingPosition.x && x < MaxPuzzleX + movingPosition.x
                && y > 0 - movingPosition.y && y < MaxPuzzleY - movingPosition.y)
            {
                return true;
            }

            return false;
        }

        public void ChangeLayer()
        {
            Scene.SceneModel.CurrentPuzzleLayer = Scene.SceneModel.GetCurrentLayer();
            Scene.SceneModel.CurrentPuzzleLayer.gameObject.SetActive(true);
        }
        public void ReloadConfiguration()
        {
            var boardConfig = Scene.GetBoardConfiguration();
            boardConfig.Clear();
            //Tile Excel => TileTemplate
            //정보를 읽고 세팅한다.
            foreach (var template in TemplateContainer<TileTemplate>.Values)
            {
                if (template.TileType != DataContainer.TileType.Normal)
                {
                    _defaultCount = 0;
                }
                else
                {
                    _defaultCount = -1;
                }
                boardConfig.AddTileDefinition(template.Name, _defaultCount);
            }
        }
        public string GetSavePath()
        {
            return $"{Consts.Path.PuzzleScene}/Output";
        }
    }
}
