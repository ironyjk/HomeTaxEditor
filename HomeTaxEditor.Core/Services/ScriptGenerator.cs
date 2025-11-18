using HomeTaxEditor.Core.Models;
using System.Text.Json;

namespace HomeTaxEditor.Core.Services;

public class ScriptGenerator
{
    /// <summary>
    /// 웹 테이블 데이터 추출 JavaScript 생성
    /// </summary>
    public string GenerateTableExtractionScript()
    {
        return @"
(function() {
    try {
        var result = {
            success: false,
            rows: [],
            error: null
        };

        // WebSquare 그리드의 모든 행 찾기
        var tableRows = document.querySelectorAll('tr.grid_body_row');

        if (tableRows.length === 0) {
            result.error = '테이블 행을 찾을 수 없습니다.';
            return JSON.stringify(result);
        }

        for (var i = 0; i < tableRows.length; i++) {
            var tr = tableRows[i];

            // rowIndex는 checkbox의 data-rowindex 속성에서 가져오기
            // (tr의 data-trindex는 항상 0이므로 사용 불가)
            var checkbox = tr.querySelector('input[type=""checkbox""]');
            var rowIndex = checkbox ? parseInt(checkbox.getAttribute('data-rowindex')) : i;

            // 각 필드 추출
            var aprvDtCell = tr.querySelector('[data-col_id=""aprvDt""]');
            var cardNoCell = tr.querySelector('[data-col_id=""busnCrdCardNoEncCntn""]');
            var bizNoCell = tr.querySelector('[data-col_id=""mrntTxprDscmNoEncCntn""]');

            // 상호명(가맹점명)
            var mrntNmCell = tr.querySelector('[data-col_id=""mrntTxprNm""]');

            var amountCell = tr.querySelector('[data-col_id=""totaTrsAmt""]');
            var ddcCell = tr.querySelector('[data-col_id=""viewDdcYnNm""]');

            if (!aprvDtCell || !cardNoCell || !bizNoCell || !amountCell) {
                continue;
            }

            var aprvDt = aprvDtCell.textContent.trim();
            var cardNo = cardNoCell.textContent.trim();
            var bizNo = bizNoCell.textContent.trim();
            var mrntNm = mrntNmCell ? mrntNmCell.textContent.trim() : '';
            var amountText = amountCell.textContent.trim().replace(/,/g, '');
            var currentDdc = ddcCell ? ddcCell.textContent.trim() : '';

            var amount = parseFloat(amountText) || 0;

            result.rows.push({
                rowIndex: rowIndex,
                aprvDt: aprvDt,
                busnCrdCardNoEncCntn: cardNo,
                mrntTxprDscmNoEncCntn: bizNo,
                mrntNm: mrntNm,
                totaTrsAmt: amount,
                currentDdcYnNm: currentDdc
            });
        }

        result.success = true;
        return JSON.stringify(result);

    } catch (ex) {
        return JSON.stringify({
            success: false,
            rows: [],
            error: ex.message
        });
    }
})();
";
    }

    /// <summary>
    /// 변경사항 적용 JavaScript 생성
    /// </summary>
    public string GenerateApplyChangesScript(List<MatchedChange> changes)
    {
        var changesJson = JsonSerializer.Serialize(changes.Select(c => new
        {
            rowIndex = c.RowIndex,
            ddcYn = c.공제여부,
            aprvDt = c.WebData.AprvDt,
            bizNo = c.WebData.MrntTxprDscmNoEncCntn,
            amount = c.WebData.TotaTrsAmt
        }).ToList());

        return $@"
(async function() {{
    try {{
        var changes = {changesJson};
        var successCount = 0;
        var debugInfo = [];

        // 각 행을 순차적으로 처리 (비동기)
        for (var i = 0; i < changes.length; i++) {{
            var change = changes[i];
            var rowIndex = change.rowIndex;
            var targetDdcYn = change.ddcYn;

            var info = {{
                rowIndex: rowIndex,
                targetValue: targetDdcYn,
                aprvDt: change.aprvDt,
                bizNo: change.bizNo,
                amount: change.amount,
                checkboxFound: false,
                selectFound: false,
                checkboxId: '',
                selectId: ''
            }};

            // 체크박스 찾기 - 여러 패턴 시도
            var checkboxPatterns = [
                'G_mf_txppWframe_grdCshpt___checkbox_chk_' + rowIndex,
                'mf_txppWframe_grdCshpt_checkbox_chk_' + rowIndex,
                'checkbox_chk_' + rowIndex
            ];

            var checkbox = null;
            for (var p = 0; p < checkboxPatterns.length; p++) {{
                checkbox = document.getElementById(checkboxPatterns[p]);
                if (checkbox) {{
                    info.checkboxFound = true;
                    info.checkboxId = checkboxPatterns[p];
                    break;
                }}
            }}

            if (!checkbox) {{
                // 대안: 해당 행의 체크박스 직접 찾기
                var row = document.querySelector('tr[data-trindex=""' + rowIndex + '""]');
                if (row) {{
                    checkbox = row.querySelector('input[type=""checkbox""]');
                    if (checkbox) {{
                        info.checkboxFound = true;
                        info.checkboxId = checkbox.id || '(no id, found by row query)';
                    }}
                }}
            }}

            if (checkbox) {{
                // 실제 클릭 이벤트 발생 (onclick 핸들러 실행)
                checkbox.click();

                // 체크박스 클릭 후 드롭다운이 활성화될 때까지 대기 (300ms)
                await new Promise(resolve => setTimeout(resolve, 300));
            }}

            // select 박스 찾기 - 여러 패턴 및 컬럼 인덱스 시도
            var selectBox = null;
            var selectPatterns = [];

            // 다양한 컬럼 인덱스 시도 (11, 12, 13, 14)
            for (var colIdx = 11; colIdx <= 14; colIdx++) {{
                selectPatterns.push('mf_txppWframe_grdCshpt_cell_' + rowIndex + '_' + colIdx + '_select_input_0');
            }}

            for (var p = 0; p < selectPatterns.length; p++) {{
                selectBox = document.getElementById(selectPatterns[p]);
                if (selectBox) {{
                    info.selectFound = true;
                    info.selectId = selectPatterns[p];
                    break;
                }}
            }}

            if (!selectBox) {{
                // 대안: 해당 행의 select 직접 찾기
                var row = document.querySelector('tr[data-trindex=""' + rowIndex + '""]');
                if (row) {{
                    var selects = row.querySelectorAll('select');
                    // 공제여부 select 찾기 (보통 마지막이거나 두번째)
                    if (selects.length > 0) {{
                        selectBox = selects[selects.length - 1];
                        info.selectFound = true;
                        info.selectId = selectBox.id || '(no id, found by row query)';
                    }}
                }}
            }}

            if (selectBox) {{
                selectBox.disabled = false;

                // 실제 사용자 동작처럼 이벤트 발생
                selectBox.focus();
                selectBox.dispatchEvent(new Event('focus', {{ bubbles: true }}));

                // 값 설정
                if (targetDdcYn === '공제') {{
                    selectBox.selectedIndex = 0;
                }} else if (targetDdcYn === '불공제') {{
                    selectBox.selectedIndex = 1;
                }}

                // 여러 이벤트 발생 (input, change)
                selectBox.dispatchEvent(new Event('input', {{ bubbles: true }}));
                selectBox.dispatchEvent(new Event('change', {{ bubbles: true }}));
                selectBox.dispatchEvent(new Event('blur', {{ bubbles: true }}));

                successCount++;
            }}

            debugInfo.push(info);
        }}

        // 저장 버튼 찾기 정보를 debugInfo에 추가
        var saveBtnInfo = {{
            saveBtnFound: false,
            saveBtnId: '',
            saveBtnText: '',
            saveBtnClicked: false,
            allButtonCount: 0
        }};

        // 저장 버튼 찾기
        var saveBtnPatterns = [
            'mf_txppWframe_trigger19',
            'mf_txppWframe_btnReg',
            'btnSave',
            'btnReg'
        ];

        var saveBtn = null;
        for (var p = 0; p < saveBtnPatterns.length; p++) {{
            saveBtn = document.getElementById(saveBtnPatterns[p]);
            if (saveBtn) {{
                saveBtnInfo.saveBtnFound = true;
                saveBtnInfo.saveBtnId = saveBtnPatterns[p];
                saveBtnInfo.saveBtnText = saveBtn.value || saveBtn.textContent.trim();
                break;
            }}
        }}

        if (!saveBtn) {{
            // ID가 mf_txppWframe_trigger로 시작하는 버튼 찾기
            var allInputs = document.querySelectorAll('input[type=""button""]');
            for (var i = 0; i < allInputs.length; i++) {{
                if (allInputs[i].id && allInputs[i].id.indexOf('mf_txppWframe_trigger') >= 0) {{
                    saveBtn = allInputs[i];
                    saveBtnInfo.saveBtnFound = true;
                    saveBtnInfo.saveBtnId = saveBtn.id;
                    saveBtnInfo.saveBtnText = saveBtn.value || '';
                    break;
                }}
            }}
        }}

        if (!saveBtn) {{
            // 텍스트로 찾기
            var buttons = document.querySelectorAll('button, a, span, input[type=""button""]');
            saveBtnInfo.allButtonCount = buttons.length;

            for (var i = 0; i < buttons.length; i++) {{
                var btnText = buttons[i].value || buttons[i].textContent.trim();
                if (btnText === '저장' || btnText === '등록' || btnText === '변경하기') {{
                    saveBtn = buttons[i];
                    saveBtnInfo.saveBtnFound = true;
                    saveBtnInfo.saveBtnId = saveBtn.id || '(found by text)';
                    saveBtnInfo.saveBtnText = btnText;
                    break;
                }}
            }}
        }}

        // 저장 버튼 정보만 수집 (클릭은 C#에서 별도로)
        if (saveBtn) {{
            console.log('[DEBUG] 저장 버튼 찾음:', saveBtnInfo.saveBtnId, saveBtnInfo.saveBtnText);
            saveBtnInfo.saveBtnClicked = false; // C#에서 클릭할 예정
        }} else {{
            console.log('[DEBUG] 저장 버튼을 찾을 수 없음. 전체 버튼 개수:', saveBtnInfo.allButtonCount);
        }}

        debugInfo.push(saveBtnInfo);

        // 결과 반환
        var result = {{
            successCount: successCount,
            debugInfo: debugInfo
        }};

        return JSON.stringify(result);

    }} catch (ex) {{
        return JSON.stringify({{
            error: ex.message,
            successCount: -1
        }});
    }}
}})();
";
    }
}
