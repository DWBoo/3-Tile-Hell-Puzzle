using Assets.Scripts.Internal;
using Dignus.Unity;
using Dignus.Unity.Framework;
using GameContents;
using System.Text;
using UnityEditor;
using UnityEngine;

namespace Assets.Scripts.Scene.PuzzleDesignScene
{
    public class PuzzleDesignScene : SceneBase
    {
        [SerializeField]
        private Camera _camera;

        public PuzzleDesignSceneController SceneController { get; private set; }
        public PuzzleDesignSceneModel SceneModel { get; private set; }

        private Vector3 _movingPos;
        private GUIStyle _labelStyle;
        private StringBuilder _tileCountText;

        [SerializeField]
        private BoardConfiguration _boardConfiguration;

        protected override void OnAwakeScene()
        {
            ApplicationManager.Instance.Init(DataContainer.BuildTaretType.Dev);
            AtlasManager.Instance.Init();
            SceneController = this.Resolve<PuzzleDesignSceneController>();
            SceneController.BindScene(this);
            SceneModel = this.Resolve<PuzzleDesignSceneModel>();

            _tileCountText = new StringBuilder();
            _labelStyle = new GUIStyle();
            _labelStyle.fontSize = 25;
#if UNITY_EDITOR
            EditorApplication.ExecuteMenuItem("Window/General/Scene");
#endif

            _boardConfiguration = new BoardConfiguration();
            SceneController.ReloadConfiguration();
        }

        protected override void OnDestroyScene()
        {
            SceneModel.Dispose();
        }
        public BoardConfiguration GetBoardConfiguration()
        {
            return _boardConfiguration;
        }
        public void OnDrawGizmos()
        {
#if UNITY_EDITOR
            Gizmos.color = Color.white;

            _movingPos = GetMovingPosition();

            for (int y = 0; y < 10; y++)
            {
                Gizmos.DrawLine(AdjustLinePos(0, y, 0f), AdjustLinePos(7, y, 0f));
            }

            for (int x = 0; x < 8; ++x)
            {
                Gizmos.DrawLine(AdjustLinePos(x, 0, 0f), AdjustLinePos(x, 9, 0f));
            }

            if (Application.isPlaying && SceneModel.CurrentPuzzleLayer != null)
            {
                _tileCountText.Clear();
                _tileCountText.Append($"타일 총 갯수: {SceneModel.TileTotalCount}\n");
                _tileCountText.Append($"[선택 된 타일: {SceneModel.GetCurrentLayerName()}] =>");
                _tileCountText.Append($"타일 수: {SceneModel.GetLayerTileCount()}");
                Handles.Label(Vector3.up * 13f, _tileCountText.ToString(), _labelStyle);

                SceneModel.CurrentPuzzleLayer.transform.position = new Vector3(_movingPos.x,
                    _movingPos.y, SceneModel.CurrentPuzzleLayer.transform.position.z);
                SceneModel.CurrentPuzzleLayer.BoardPosition = SceneModel.GetBoardPosition();
            }
#endif
        }
        private Vector3 AdjustLinePos(float x, float y, float z)
        {
            return new Vector3(x, y, z) + _movingPos;
        }

        public Vector3 GetMovingPosition()
        {
            if (Application.isPlaying && SceneModel.GetBoardPositionDicCount() > 0)
            {
                switch (SceneModel.GetBoardPosition())
                {
                    case BoardPosition.Up:
                        return new Vector3(0f, 0.5f, 0f);

                    case BoardPosition.Down:
                        return new Vector3(0f, -0.5f, 0f);

                    case BoardPosition.Left:
                        return new Vector3(-0.5f, 0f, 0f);

                    case BoardPosition.Right:
                        return new Vector3(0.5f, 0f, 0f);

                    case BoardPosition.DiagonalUpRight:
                        return new Vector3(0.5f, 0.5f, 0f);

                    case BoardPosition.DiagonalUpLeft:
                        return new Vector3(-0.5f, 0.5f, 0f);

                    case BoardPosition.DiagonalDownLeft:
                        return new Vector3(-0.5f, -0.5f, 0f);

                    case BoardPosition.DiagonalDownRight:
                        return new Vector3(0.5f, -0.5f, 0f);

                    default:
                        return Vector3.zero;
                }
            }

            return Vector3.zero;
        }

        public Camera GetCemara()
        {
            return _camera;
        }
    }
}
