import os

def fix_unity_conflict(input_filename, output_filename):
    """
    Unity YAML 파일의 꼬인 Merge Conflict를 해결합니다.
    - 'Stashed changes' 내용을 우선하여 반영합니다.
    - 중첩되거나 중복된 충돌 마커를 제거합니다.
    """
    
    # 상태 상수
    STATE_NORMAL = 0
    STATE_UPSTREAM = 1 # <<<<<<< 블록 내부 (버릴 내용)
    STATE_STASH = 2    # ======= 이후 블록 (가져올 내용)

    current_state = STATE_NORMAL
    
    # 처리 통계
    processed_lines = 0
    conflict_blocks = 0
    
    with open(input_filename, 'r', encoding='utf-8') as infile, \
         open(output_filename, 'w', encoding='utf-8') as outfile:
        
        for line in infile:
            stripped = line.strip()
            
            # 1. 충돌 시작 마커 감지 (<<<<<<< Updated upstream)
            if stripped.startswith("<<<<<<<"):
                if current_state == STATE_NORMAL:
                    current_state = STATE_UPSTREAM
                    conflict_blocks += 1
                # 이미 UPSTREAM이나 STASH 상태라면 중첩된 마커이므로 무시하고 넘어감
                continue

            # 2. 구분자 마커 감지 (=======)
            if stripped.startswith("======="):
                if current_state == STATE_UPSTREAM:
                    current_state = STATE_STASH
                # STASH 상태에서 또 ======= 가 나오면 중복 마커이므로 무시
                # NORMAL 상태에서 나오면 잘못된 마커(쓰레기 값)이므로 무시
                continue

            # 3. 종료 마커 감지 (>>>>>>> Stashed changes)
            if stripped.startswith(">>>>>>>"):
                if current_state == STATE_STASH or current_state == STATE_UPSTREAM:
                    current_state = STATE_NORMAL
                # NORMAL 상태에서 나오면 잘못된 마커이므로 무시
                continue

            # 4. 내용 출력 (현재 상태에 따라 결정)
            if current_state == STATE_NORMAL:
                outfile.write(line)
            elif current_state == STATE_STASH:
                # STASH 상태여도 내부의 또다른 충돌 마커 라인은 건너뜀 (내용만 취함)
                if not (stripped.startswith("<<<<<<<") or stripped.startswith("=======") or stripped.startswith(">>>>>>>")):
                    outfile.write(line)
            # STATE_UPSTREAM 일 때는 내용을 쓰지 않고 버림

    print(f"작업 완료: {output_filename}")
    print(f"총 {conflict_blocks}개의 충돌 블록을 Stash 기준으로 정리했습니다.")

# 실행
input_file = "TestCombatScene5.unity"
output_file = "TestCombatScene5_Fixed.unity"

if os.path.exists(input_file):
    fix_unity_conflict(input_file, output_file)
else:
    print(f"오류: '{input_file}' 파일을 찾을 수 없습니다. 스크립트와 같은 폴더에 파일을 놓아주세요.")