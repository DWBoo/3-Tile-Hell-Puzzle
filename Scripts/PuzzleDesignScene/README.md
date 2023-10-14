# 개요
기획자가 게임 스테이지를 구성할 수 있도록 타일을 배치 및 수정하는 씬

# 기능 설명
![Scene](https://github.com/DWBoo/3-Tile-Hell-Puzzle/assets/147593910/affbd239-3196-49e6-963f-6bbb3ab11b50)
![Inspector](https://github.com/DWBoo/3-Tile-Hell-Puzzle/assets/147593910/e5c958ba-ae6b-417e-aa0f-b04ac0b7045e)
![Hierarchy](https://github.com/DWBoo/3-Tile-Hell-Puzzle/assets/147593910/8b80a522-a0ba-43c4-9111-015e0b36f0aa)</br>

1. 레이어 생성</br>
   타일이 배치될 레이어를 생성, 타일이 배치된 여러 층의 레이어가 하나의 스테이지

2. 타일 배치</br>
   Scene 에디터에서 격자를 클릭하며 원하는 대로 타일 배치

3. 레이어 위치 조정</br>
   다양한 보드 모양을 내기 위한 레이어(격자) 위치 조정

4. 레이어 선택</br>
   수정을 위해 생성된 다른 레이어로 이동

5. 겹침 타일</br>
   최근 배치한 타일을 기준, 한 방향으로 계속 겹처있는 타일 배치

6. 저장</br>
   스테이지의 이름을 입력 후 프리팹으로 저장

7. 불러오기</br>
   수정을 위해 프리팹으로 생성된 이름을 입력 후 불러오기

# 요약
![image](https://github.com/DWBoo/3-Tile-Hell-Puzzle/assets/147593910/c251fd37-b8e4-4ab4-9f5f-0390a50475ec)
타일이 배치되어 있는 레이어가 여러 층으로 구성되어 있기에 레이어 별로 보드(격자)의 위치를 관리하기 위해 Dictionary 자료 구조 사용
![image](https://github.com/DWBoo/3-Tile-Hell-Puzzle/assets/147593910/7bd9f302-56f3-40c9-acfc-4b23196ded89)
레이어를 추가하면 이름만 관리하는 List에 저장
![image](https://github.com/DWBoo/3-Tile-Hell-Puzzle/assets/147593910/0540f435-69d1-4dad-ae61-ccc5581440ae)
![image](https://github.com/DWBoo/3-Tile-Hell-Puzzle/assets/147593910/3fd78346-d1b2-4a16-93db-27cb79699b48)
목록에서 선택한 Index를 이용하여 현재 선택 한 레이어의 위치를 반환
![image](https://github.com/DWBoo/3-Tile-Hell-Puzzle/assets/147593910/d87ca39e-9048-4d06-9de2-46ea53170718)
고정된 위치로 위치 별 조정 값은 하드 코딩 되어 선택된 위치에 맞게 선을 그려 격자 판을 생성
![image](https://github.com/DWBoo/3-Tile-Hell-Puzzle/assets/147593910/24640ec5-771d-430f-90e9-ecf70231f36d)
범위 내 클릭할 경우 격자의 위치에 맞게 타일을 배치하기 위해 타일의
위치도 조정하고 이미 배치된 타일을 다시 클릭하게 되면 배치된 타일 제거
이러한 타일 배치를 중점으로 스테이지를 구성하는데 필요한 부가 기능을 추가로 구현 
