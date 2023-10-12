<p align="center">
  헬 퍼즐(3-타일 퍼즐 게임)
</p>

# [타일 배치 씬](https://github.com/DWBoo/3-Tile-Hell-Puzzle/tree/main/Scripts/PuzzleDesignScene)
게임에 사용 되는 씬들은 모두 MVC 패턴을 사용 (설계 및 구현이 되어 있는 상태에서 디자인 기능만 구현)
* SceneController는 로직을 담당
* Model은 로직 처리 후 갱신이 필요한 정보나 데이터를 담는데 사용
* Model의 정보를 가져와서 갱신하여 화면에 표시</br></br>

# [게임 매니저](https://github.com/DWBoo/3-Tile-Hell-Puzzle/tree/main/Scripts/PuzzleScene)
설계 및 구현이 되어 있는 코드에서 처리에 맞는 분기에 연출만 구현</br>

# [설정 팝업](https://github.com/DWBoo/3-Tile-Hell-Puzzle/tree/main/Scripts/MainScene/UI)
효과음 및 배경음 음소거 설정

# [결과 팝업](https://github.com/DWBoo/3-Tile-Hell-Puzzle/tree/main/Scripts/PuzzleScene/UI)
게임 클리어 혹은 실패 시 결과 팝업 활성화
