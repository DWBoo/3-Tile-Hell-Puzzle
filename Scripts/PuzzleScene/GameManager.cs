using Assets.Scripts.Internal.Audio;
using Assets.Scripts.Scene.PuzzleScene;
using DataContainer.Generated;
using Dignus.Collections;
using Dignus.Log;
using Dignus.Unity;
using Dignus.Unity.Coroutine;
using GameContents;
using System.Collections;
using TemplateContainers;
using UnityEngine;
using UnityEngine.EventSystems;
namespace Assets.Scripts.Internal
{
    public class GameManager : MonoBehaviour
    {
        public event AddTrackSlotClickedHandler AddTrackSlotClicked;
        public delegate void AddTrackSlotClickedHandler(GameObject go);

        [SerializeField]
        private Camera _gameCamera;

        private PuzzleBoard _puzzleBoard;
        private PuzzleEventHandler _puzzleEventHandler;
        private bool _isGameRunning = true;
        private PuzzleBoardObject _puzzleBoardGo;
        private PuzzleSceneController _puzzleSceneController;
        private readonly ArrayQueue<IPuzzleEvent> _puzzleEvents = new();

        private GameObject _coinEffect;
        private GameObject _bombEffect;

        private Vector3 _latestRemovedPosition;
        private int _pointId = 0;

        //임시 삭제 예정
        [SerializeField]
        private bool _isClear;

        private void Awake()
        {
#if UNITY_EDITOR || UNITY_STANDALONE
            _pointId = -1;
#endif
        }
        public void Init(PuzzleBoardObject puzzleBoardObject,
            PuzzleSceneController puzzleSceneController)
        {
            _puzzleSceneController = puzzleSceneController;
            _puzzleBoard = puzzleBoardObject.PuzzleBoard;
            _puzzleBoardGo = puzzleBoardObject;
            _puzzleEventHandler = _puzzleBoard.GetEventHandler() as PuzzleEventHandler;

            var coinEffectPrefab = DignusUnityResourceManager.Instance.LoadAsset<GameObject>(Consts.Path.PuzzleSceneEffectCoin);
            _coinEffect = DignusUnityObjectPool.Instance.Pop(coinEffectPrefab);
            _coinEffect.SetActive(false);
            _coinEffect.transform.SetParent(transform, false);

            var bombEffectPrefab = DignusUnityResourceManager.Instance.LoadAsset<GameObject>(Consts.Path.PuzzleSceneEffectBomb);
            _bombEffect = DignusUnityObjectPool.Instance.Pop(bombEffectPrefab);
            _bombEffect.SetActive(false);
            _bombEffect.transform.SetParent(transform, false);
        }
        public PuzzleBoardObject GetPuzzleBoardObject()
        {
            return _puzzleBoardGo;
        }
        private IEnumerator ProcessGameEvents()
        {
            while (_isGameRunning)
            {
                _puzzleEvents.AddRange(_puzzleEventHandler.GetPuzzleEvents());
                _puzzleEventHandler.ClearEvents();

                while (_puzzleEvents.Count > 0)
                {
                    ProcessEvent(_puzzleEvents.Read());
                }
                yield return null;
            }
        }

        public void StartGame()
        {
            _isGameRunning = true;
            _puzzleBoard.StartGame();
            DignusUnityCoroutineManager.Start(ProcessGameEvents());

            //임시 테스트용 (삭제예정)
            if (_isClear)
            {
                GameClear();
            }
        }
        public void StopGame()
        {
            _isGameRunning = false;
        }
        public void Pause()
        {
            _puzzleBoard.Pause();
        }
        public void Resume()
        {
            _puzzleBoard.Resume();
        }
        public bool IsRunning()
        {
            return _isGameRunning;
        }

        public void Update()
        {
            if (Input.GetMouseButtonDown(0))
            {
                if (EventSystem.current.IsPointerOverGameObject(_pointId))
                {
                    return;
                }
                var pos = _gameCamera.ScreenToWorldPoint(Input.mousePosition);
                var hit = Physics2D.Raycast(pos, Vector2.zero);
                if (hit.collider != null)
                {
                    if (hit.collider.gameObject.Equals(_puzzleBoardGo.GetTrackLayerObject().GetAddTrackSlotGo()))
                    {
                        AddTrackSlotClicked?.Invoke(_puzzleBoardGo.GetTrackLayerObject().GetAddTrackSlotGo());
                        return;
                    }
                    var tileGo = hit.collider.gameObject.GetComponent<PuzzleTileObject>();
                    if (tileGo.IsTileActive() == false)
                    {
                        LogHelper.Info($"[InGame] isActive ? {tileGo.GetPuzzleTile().IsActive} X : {tileGo.GetPuzzleTile().X} Y : {tileGo.GetPuzzleTile().Y} layer : {tileGo.GetPuzzleLayer().PuzzleLayer.LayerIndex}");
                        return;
                    }
                    _puzzleBoard.RemoveBoardTile(tileGo.GetPuzzleTile());
                }
                else
                {
                    LogHelper.Info($"[InGame]{pos}");
                }
            }
        }
        private void ProcessEvent(IPuzzleEvent puzzleEvent)
        {
            LogHelper.Debug($"[InGame]{puzzleEvent}");
            if (puzzleEvent == null)
            {
                return;
            }
            if (puzzleEvent is ActivePuzzleTile activePuzzleTile)
            {
                var layer = _puzzleBoardGo.GetPuzzleLayerObject(activePuzzleTile.PuzzleTile.PuzzleLayer);
                layer.Refresh(activePuzzleTile.PuzzleTile);
            }
            else if (puzzleEvent is RemovePuzzleTile removePuzzleTile)
            {
                AudioManager.Instance.PlayOneShot(Audio.AudioType.SFX_Button);
                var layerGo = _puzzleBoardGo.GetPuzzleLayerObject(removePuzzleTile.PuzzleTile.PuzzleLayer);
                var puzzleTileGo = layerGo.GetPuzzleTileObject(removePuzzleTile.PuzzleTile);
                _latestRemovedPosition = puzzleTileGo.transform.position;
                layerGo.RemovePuzzleTile(puzzleTileGo.GetPuzzleTile());
            }
            else if (puzzleEvent is AddTrackPuzzleTile addTrackPuzzleTile)
            {
                _puzzleBoardGo.MovePuzzleTileToTrack(addTrackPuzzleTile);
            }
            else if (puzzleEvent is RemoveTrackTile removeTrackPuzzleTile)
            {
                _puzzleBoardGo.DisappearTrackTile(removeTrackPuzzleTile.RemovedTrackTiles);
            }
            else if (puzzleEvent is GameOver gameOver)
            {
                _puzzleBoardGo.GameOver(gameOver);
            }
            else if (puzzleEvent is GameClear gameClear)
            {
                _puzzleBoardGo.GameClear(gameClear);
            }
            else if (puzzleEvent is AcquireGold acquireGold)
            {
                //단순 연출 필요??
                _puzzleSceneController.Scene.SceneModel.TotalAcquireGold += acquireGold.Amount;
                _puzzleSceneController.Scene.RefreshCoin();
            }
            else if (puzzleEvent is IncreaseTrackSlot)
            {

            }
            else if (puzzleEvent is ShuffleBoard shuffleBoard)
            {
                //셔플
                _puzzleBoardGo.ShuffleBoard(shuffleBoard, 0);
            }
            else if (puzzleEvent is RevertTile revertTile)
            {
                //되돌리기
                _puzzleBoardGo.RevertTrackTile(revertTile, _latestRemovedPosition);
                for (int i = 0; i < _puzzleEvents.Count; ++i)
                {
                    if (_puzzleEvents[i] is ActivePuzzleTile activePuzzleEvent)
                    {
                        if (revertTile.AddPuzzleTile == activePuzzleEvent.PuzzleTile)
                        {
                            _puzzleEvents[i] = null;
                            break;
                        }
                    }
                }
            }
            else if (puzzleEvent is ExtractTrackTile extractTrackTile)
            {
                //꺼내기
                _puzzleBoardGo.ExtractTrackTile(extractTrackTile);
                for (int i = 0; i < extractTrackTile.TileExtractions.Count; ++i)
                {
                    var addedPuzzleTile = extractTrackTile.TileExtractions[i].AddPuzzleTile;
                    for (int ii = 0; ii < _puzzleEvents.Count; ++ii)
                    {
                        if (_puzzleEvents[ii] is ActivePuzzleTile activePuzzleEvent)
                        {
                            if (addedPuzzleTile == activePuzzleEvent.PuzzleTile)
                            {
                                _puzzleEvents[ii] = null;
                                break;
                            }
                        }
                    }
                }
            }
            else if (puzzleEvent is InactivePuzzleTile inactivePuzzleTile)
            {
                var layerGo = _puzzleBoardGo.GetPuzzleLayerObject(inactivePuzzleTile.PuzzleTile.PuzzleLayer);
                layerGo.Refresh(inactivePuzzleTile.PuzzleTile);
            }
            else if (puzzleEvent is StartOfSpecialTileAction startOfSpecialTileAction)
            {
                var template = TemplateContainer<TileTemplate>.Find(startOfSpecialTileAction.TemplateId);

                if (template.TileType == DataContainer.TileType.BombAction)
                {
                    _puzzleBoardGo.BombTile(_bombEffect);
                    //특수 타일 연출 아래 순서로 이벤트 삽입
                    //startOfSpecialTileAction
                    //AcquireGold Event
                    //RemoveTrackPuzzleTile Event
                    //endOfSpecialTileAction

                }
                else if (template.TileType == DataContainer.TileType.CoinAction)
                {
                    //특수 타일 연출 아래 순서로 이벤트 삽입
                    //StartOfSpecialTileAction
                    //AcquireGold Event가 넘어옴
                    //EndOfSpecialTileAction

                    if (_puzzleEvents.Peek() is AcquireGold)
                    {
                        var acquireGoldEvt = _puzzleEvents.Read() as AcquireGold;
                        _puzzleSceneController.Scene.SceneModel.TotalAcquireGold += acquireGoldEvt.Amount;
                    }
                    _puzzleBoardGo.AcquireCoin(_coinEffect);
                }
                else if (template.TileType == DataContainer.TileType.ShuffleAction)
                {
                    //특수 타일 연출 아래 순서로 이벤트 삽입
                    var shuffleBoardEvt = _puzzleEvents.Read();
                    _puzzleBoardGo.ShuffleBoard(shuffleBoardEvt as ShuffleBoard,
                        Consts.TileMovementDuration);

                    //startOfSpecialTileAction

                    //ShuffleBoard Event가 넘어옴

                    //endOfSpecialTileAction
                }
            }
            else if (puzzleEvent is EndOfSpecialTileAction)
            {
                //특수 타일 이벤트 끝

            }
            else if (puzzleEvent is RetryGame)
            {

            }
            else
            {
                LogHelper.Debug($"[InGame]unprocessed puzzle event detected");
            }
        }
        public void Dispose()
        {
            _puzzleSceneController.PlayerManager.UpdateGold(_puzzleSceneController.Scene.SceneModel.TotalAcquireGold);

            _puzzleSceneController.Scene.SceneModel.TotalAcquireGold = 0;

            _puzzleBoardGo.Dispose();
            _puzzleBoardGo.Recycle();
            _coinEffect.Recycle();
            _coinEffect = null;
            _bombEffect.Recycle();
            _bombEffect = null;
        }

        //임시 테스트용 (삭제 예정)
        public void GameClear()
        {
            var removeEvent = new RemoveTrackTile();
            _puzzleEventHandler.Process(removeEvent);
            _puzzleEventHandler.Process(new GameClear()
            {
                TotalAcquireCoin = 0
            });
        }
    }
}