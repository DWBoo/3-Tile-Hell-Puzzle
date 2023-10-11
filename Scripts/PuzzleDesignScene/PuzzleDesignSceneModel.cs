using Assets.Scripts.Scene.PuzzleScene;
using Dignus.Log;
using Dignus.Unity;
using Dignus.Unity.Framework;
using GameContents;
using System.Collections.Generic;
using UnityEngine;

namespace Assets.Scripts.Scene.PuzzleDesignScene
{
    public class PuzzleDesignSceneModel : ISceneModel
    {
        public List<PuzzleLayerObject> Layers { get; set; } = new List<PuzzleLayerObject>();
        //레이어를 추가 생성하게 될 경우 레이어 별로 타일을 저장하기 위해 Dictionary 사용.
        //CurrentPuzzleLayer에 현재 Layer를 항상 업데이트 하기에 해당 필드로 현재 레이어의 타일에 접근.
        public Dictionary<PuzzleLayerObject, List<PuzzleTileObject>> TilesDic = new();
        //인스펙터 UI에서 레이어를 선택을 위한 리스트에 표시 될 이름, 생성 순서대로 Layer_xx 숫자 부여.
        public List<string> LayerNameList = new();
        public Dictionary<string, int> TileCountDic = new();
        public PuzzleBoardObject PuzzleBoard { get; set; }
        public PuzzleLayerObject CurrentPuzzleLayer { get; set; }

        public int LayerIndex;
        public int TileTotalCount;

        private readonly Dictionary<string, BoardPosition> CurrentBoardPositionDic = new();

        public int GetLayerTileCount()
        {
            return TileCountDic[LayerNameList[LayerIndex]];
        }

        public PuzzleLayerObject GetCurrentLayer()
        {
            return Layers[LayerIndex];
        }

        public string GetCurrentLayerName()
        {
            return LayerNameList[LayerIndex];
        }

        public BoardPosition GetBoardPosition()
        {
            return CurrentBoardPositionDic[LayerNameList[LayerIndex]];
        }

        public void SetBoardPosition(BoardPosition boardPosition)
        {
            CurrentBoardPositionDic[LayerNameList[LayerIndex]] = boardPosition;
        }

        public void AddBoardPosition(BoardPosition boardPosition)
        {
            CurrentBoardPositionDic.Add(LayerNameList[LayerIndex], boardPosition);
        }

        public int GetBoardPositionDicCount()
        {
            return CurrentBoardPositionDic.Count;
        }

        public bool TryRemovingTile(Vector3 mousePos, BoardPosition boardPosition, out PuzzleTileObject tile)
        {
            switch (boardPosition)
            {
                case BoardPosition.Up:
                    mousePos -= new Vector3(0f, -0.5f, 0f);
                    break;

                case BoardPosition.Down:
                    mousePos -= new Vector3(0f, 0.5f, 0f);
                    break;

                case BoardPosition.Left:
                    mousePos += new Vector3(0.5f, 0f, 0f);
                    break;

                case BoardPosition.Right:
                    mousePos += new Vector3(-0.5f, 0f, 0f);
                    break;

                case BoardPosition.DiagonalUpRight:
                    mousePos = new Vector3(mousePos.x + (-0.5f), mousePos.y - (-0.5f), 0f);
                    break;

                case BoardPosition.DiagonalUpLeft:
                    mousePos = new Vector3(mousePos.x + 0.5f, mousePos.y - (-0.5f), 0f);
                    break;

                case BoardPosition.DiagonalDownLeft:
                    mousePos = new Vector3(mousePos.x + 0.5f, mousePos.y - 0.5f, 0f);
                    break;

                case BoardPosition.DiagonalDownRight:
                    mousePos = new Vector3(mousePos.x + (-0.5f), mousePos.y - 0.5f, 0f);
                    break;

                default:
                    break;
            }

            tile = null;
            foreach (var t in TilesDic[CurrentPuzzleLayer])
            {
                if (t.GridTilePos == mousePos)
                {
                    tile = t;
                    break;
                }
            }

            if (!tile.IsNull())
            {
                TilesDic[CurrentPuzzleLayer].Remove(tile);

                try
                {
                    tile.Recycle();
                }
                catch (KeyNotFoundException ex)
                {
                    LogHelper.Info(ex.Message);
                }

                return true;
            }

            return false;
        }

        public void Dispose()
        {
            foreach (var key in TilesDic.Keys)
            {
                TilesDic[key].ForEach((tile) => tile.Recycle());
            }
            TilesDic.Clear();

            Layers.ForEach((layer) => layer.Recycle());
            Layers.Clear();

            PuzzleBoard.Recycle();
            LayerNameList.Clear();
            TileCountDic.Clear();
            TileTotalCount = 0;
        }
    }
}
