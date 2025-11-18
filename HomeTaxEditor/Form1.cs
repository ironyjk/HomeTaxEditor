using Microsoft.Web.WebView2.Core;
using System.Text.Json;
using System.Data;
using HomeTaxEditor.Core.Services;
using HomeTaxEditor.Core.Models;

namespace HomeTaxEditor;

public partial class Form1 : Form
{
    private const string HOMETAX_URL = "https://www.hometax.go.kr/";
    private bool isWebViewInitialized = false;
    private bool isDeveloperMode = false;
    private string? selectedExcelPath = null;
    private DataTable changesTable = new DataTable();
    private CancellationTokenSource? _cancellationTokenSource = null;

    // Core 서비스
    private readonly ExcelReader _excelReader = new ExcelReader();
    private readonly DataMatcher _dataMatcher = new DataMatcher();
    private readonly ScriptGenerator _scriptGenerator = new ScriptGenerator();

    public Form1(bool developerMode = false)
    {
        isDeveloperMode = developerMode;
        InitializeComponent();
        InitializeChangesTable();

        // 개발 모드일 경우 개발자 도구 패널 표시
        if (isDeveloperMode)
        {
            lblDevMode.Visible = true;
            panelDevTools.Visible = true;
            this.Text += " [개발자 모드]";
        }
    }

    private async void Form1_Load(object sender, EventArgs e)
    {
        await InitializeWebView2();
    }

    private async Task InitializeWebView2()
    {
        try
        {
            LogMessage("WebView2 초기화 중...");

            // WebView2 환경 설정
            var env = await CoreWebView2Environment.CreateAsync(null, Path.Combine(Path.GetTempPath(), "HomeTaxEditor"));
            await webView.EnsureCoreWebView2Async(env);

            // 웹 메시지 수신 핸들러 등록
            webView.CoreWebView2.WebMessageReceived += CoreWebView2_WebMessageReceived;

            // 네비게이션 완료 이벤트 핸들러
            webView.CoreWebView2.NavigationCompleted += CoreWebView2_NavigationCompleted;

            isWebViewInitialized = true;
            LogMessage("WebView2 초기화 완료");

            // 자동으로 홈택스 접속
            LogMessage("홈택스 접속 중...");
            webView.CoreWebView2.Navigate(HOMETAX_URL);
        }
        catch (Exception ex)
        {
            LogMessage($"WebView2 초기화 오류: {ex.Message}");
        }
    }

    private void CoreWebView2_NavigationCompleted(object? sender, CoreWebView2NavigationCompletedEventArgs e)
    {
        if (e.IsSuccess)
        {
            LogMessage($"페이지 로드 완료: {webView.CoreWebView2.Source}");
        }
        else
        {
            LogMessage($"페이지 로드 실패: {e.WebErrorStatus}");
        }
    }

    private void CoreWebView2_WebMessageReceived(object? sender, CoreWebView2WebMessageReceivedEventArgs e)
    {
        try
        {
            var message = e.TryGetWebMessageAsString();

            // 먼저 메뉴 네비게이션 결과인지 확인
            try
            {
                var navResult = JsonSerializer.Deserialize<NavigationResult>(message);
                if (navResult != null && !string.IsNullOrEmpty(navResult.currentStep))
                {
                    LogMessage($"[메뉴 진입] {navResult.currentStep}");

                    if (!navResult.success && navResult.error != null)
                    {
                        LogMessage($"[메뉴 진입 실패] {navResult.error}");
                    }
                    else if (navResult.success)
                    {
                        LogMessage("[메뉴 진입 완료] 매입세액 공제 확인/변경 화면으로 이동했습니다.");
                    }
                    return;
                }
            }
            catch { }

            // DOM 테스트 결과 처리
            var result = JsonSerializer.Deserialize<DOMTestResult>(message);
            if (result != null)
            {
                if (result.success)
                {
                    LogMessage("=== DOM 테스트 결과 ===");
                    LogMessage($"테이블 행 개수: {result.rowCount}");
                    LogMessage($"첫 번째 행 텍스트: {result.firstRowText}");
                    LogMessage($"DOM 수정 성공: {result.modificationSuccess}");
                    LogMessage("=====================");
                }
                else
                {
                    LogMessage($"DOM 테스트 실패: {result.error}");
                }
            }
        }
        catch (Exception ex)
        {
            LogMessage($"메시지 처리 오류: {ex.Message}");
        }
    }

    private async void btnTestDOM_Click(object sender, EventArgs e)
    {
        if (!isWebViewInitialized)
        {
            LogMessage("WebView2가 아직 초기화되지 않았습니다.");
            return;
        }

        try
        {
            LogMessage("DOM 테스트 실행 중...");

            // JavaScript 코드 실행
            string script = @"
                (function() {
                    try {
                        var result = {
                            success: true,
                            rowCount: 0,
                            firstRowText: '',
                            modificationSuccess: false,
                            error: null
                        };

                        // 다양한 테이블 셀렉터 시도
                        var tables = document.querySelectorAll('table');
                        var targetTable = null;

                        // 가장 큰 테이블 찾기 (데이터 테이블일 가능성이 높음)
                        var maxRows = 0;
                        for (var i = 0; i < tables.length; i++) {
                            var rows = tables[i].querySelectorAll('tr');
                            if (rows.length > maxRows) {
                                maxRows = rows.length;
                                targetTable = tables[i];
                            }
                        }

                        if (targetTable) {
                            var rows = targetTable.querySelectorAll('tbody tr, tr');
                            result.rowCount = rows.length;

                            if (rows.length > 0) {
                                // 첫 번째 행의 텍스트 읽기
                                var firstRow = rows[0];
                                result.firstRowText = firstRow.textContent.trim().substring(0, 100);

                                // DOM 수정: 첫 번째 행 배경색 변경
                                firstRow.style.backgroundColor = '#FFFF99';
                                firstRow.style.border = '2px solid #FF6600';

                                // 체크박스가 있는 경우 찾아서 상태 변경
                                var checkboxes = firstRow.querySelectorAll('input[type=""checkbox""]');
                                if (checkboxes.length > 0) {
                                    checkboxes[0].checked = !checkboxes[0].checked;
                                }

                                // 라디오 버튼이 있는 경우 찾아서 상태 변경
                                var radios = firstRow.querySelectorAll('input[type=""radio""]');
                                if (radios.length > 0) {
                                    radios[0].checked = true;
                                }

                                result.modificationSuccess = true;
                            }
                        } else {
                            result.success = false;
                            result.error = '페이지에서 테이블을 찾을 수 없습니다. 로그인 및 대상 페이지 진입 후 다시 시도하세요.';
                        }

                        // C#으로 결과 전송
                        window.chrome.webview.postMessage(JSON.stringify(result));
                    } catch (ex) {
                        var errorResult = {
                            success: false,
                            rowCount: 0,
                            firstRowText: '',
                            modificationSuccess: false,
                            error: ex.message
                        };
                        window.chrome.webview.postMessage(JSON.stringify(errorResult));
                    }
                })();
            ";

            await webView.CoreWebView2.ExecuteScriptAsync(script);
        }
        catch (Exception ex)
        {
            LogMessage($"DOM 테스트 오류: {ex.Message}");
        }
    }

    private void InitializeChangesTable()
    {
        changesTable = new DataTable();
        changesTable.Columns.Add("날짜", typeof(string));
        changesTable.Columns.Add("가맹점사업자번호", typeof(string));
        changesTable.Columns.Add("상호명", typeof(string));
        changesTable.Columns.Add("금액", typeof(string));
        changesTable.Columns.Add("변경 전", typeof(string));
        changesTable.Columns.Add("변경 후", typeof(string));
        changesTable.Columns.Add("상태", typeof(string));

        dataGridChanges.DataSource = changesTable;
    }

    private async void btnStartProcess_Click(object sender, EventArgs e)
    {
        // 엑셀 파일 선택
        using (OpenFileDialog openFileDialog = new OpenFileDialog())
        {
            openFileDialog.Filter = "Excel Files|*.xlsx;*.xls|All Files|*.*";
            openFileDialog.Title = "엑셀 파일 선택";

            if (openFileDialog.ShowDialog() != DialogResult.OK)
            {
                return;
            }

            selectedExcelPath = openFileDialog.FileName;
            LogMessage($"엑셀 파일 선택: {selectedExcelPath}");
        }

        // 자동 반영 시작
        if (!isWebViewInitialized)
        {
            MessageBox.Show("홈택스에 먼저 접속하세요.", "알림", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            return;
        }

        // CancellationTokenSource 생성
        _cancellationTokenSource = new CancellationTokenSource();

        // UI 상태 변경
        btnStartProcess.Enabled = false;
        btnStop.Enabled = true;

        try
        {
            await RunAutoReflectProcess(_cancellationTokenSource.Token);
        }
        catch (OperationCanceledException)
        {
            LogMessage("=== 사용자가 작업을 중단했습니다 ===");
            MessageBox.Show("작업이 중단되었습니다.", "중단", MessageBoxButtons.OK, MessageBoxIcon.Information);
        }
        finally
        {
            // UI 상태 복구
            btnStartProcess.Enabled = true;
            btnStop.Enabled = false;
            _cancellationTokenSource?.Dispose();
            _cancellationTokenSource = null;
        }
    }

    private void btnStop_Click(object sender, EventArgs e)
    {
        if (_cancellationTokenSource != null)
        {
            LogMessage("작업 중단 요청...");
            _cancellationTokenSource.Cancel();
            btnStop.Enabled = false;
        }
    }

    private async Task RunAutoReflectProcess(CancellationToken cancellationToken)
    {
        try
        {
            LogMessage("=== 자동 반영 프로세스 시작 ===");
            progressBar.Value = 0;
            progressBar.Maximum = 100;

            // 1단계: 메뉴 자동 진입 (20%)
            LogMessage("[1/6] 메뉴 자동 진입 중...");
            cancellationToken.ThrowIfCancellationRequested();
            await NavigateToCardMenu();
            await Task.Delay(5000, cancellationToken); // 페이지 로딩 대기 (5초)
            progressBar.Value = 20;

            // 2단계: 엑셀 파일 읽기 (30%)
            LogMessage("[2/6] 엑셀 파일 읽기 중...");
            cancellationToken.ThrowIfCancellationRequested();
            var excelData = _excelReader.ReadExcelFile(selectedExcelPath!);
            if (excelData.Count == 0)
            {
                LogMessage("엑셀 파일에서 데이터를 읽을 수 없습니다.");
                MessageBox.Show("엑셀 파일에서 데이터를 읽을 수 없습니다.", "오류", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }
            LogMessage($"엑셀에서 {excelData.Count}건의 데이터를 읽었습니다.");
            progressBar.Value = 30;

            // 3단계: 날짜 범위 추출 및 조회 기간 설정 (50%)
            LogMessage("[3/6] 조회 기간 설정 중...");
            cancellationToken.ThrowIfCancellationRequested();
            var (year, quarter) = GetDateRangeFromExcelData(excelData);
            LogMessage($"데이터 범위: {year}년 {quarter}분기");
            await SetSearchPeriodAndSearch(year, quarter);
            await Task.Delay(3000, cancellationToken); // 검색 결과 로딩 대기 (3초)
            progressBar.Value = 50;

            // 4-6단계: 모든 페이지 처리 (60% ~ 100%)
            LogMessage("[4/6] 모든 페이지를 처리합니다");
            changesTable.Rows.Clear();

            int totalAppliedCount = 0;
            int totalMatchedCount = 0;
            int pageNumber = 1;
            bool hasMorePages = true;

            while (hasMorePages)
            {
                cancellationToken.ThrowIfCancellationRequested();
                LogMessage($"--- 페이지 {pageNumber} 처리 중 ---");

                // 현재 페이지 데이터 추출
                var webData = await ExtractWebTableData();
                if (webData.Count == 0)
                {
                    LogMessage($"페이지 {pageNumber}에서 데이터를 추출할 수 없습니다. 모든 페이지 처리 완료.");
                    hasMorePages = false;
                    break;
                }

                LogMessage($"페이지 {pageNumber}: {webData.Count}건 추출");

                // 데이터 매칭
                var matchedChanges = _dataMatcher.MatchData(excelData, webData);
                var (total, matched, needChange) = _dataMatcher.GetMatchingStats(excelData, webData, matchedChanges);

                totalMatchedCount += matched;
                LogMessage($"페이지 {pageNumber}: {matched}건 매칭, {needChange}건 변경 필요");

                // 변경 내역 테이블에 추가
                foreach (var change in matchedChanges)
                {
                    changesTable.Rows.Add(
                        change.ExcelData.승인일자, // 날짜
                        change.ExcelData.가맹점사업자번호, // 가맹점사업자번호
                        change.WebData.MrntNm, // 상호명
                        change.ExcelData.합계.ToString("N0"), // 금액
                        change.WebData.CurrentDdcYnNm, // 변경 전
                        change.공제여부, // 변경 후
                        "대기" // 상태
                    );
                }

                // 변경사항 적용
                if (matchedChanges.Count > 0)
                {
                    cancellationToken.ThrowIfCancellationRequested();
                    LogMessage($"페이지 {pageNumber}: {matchedChanges.Count}건 변경 적용 중...");
                    int appliedCount = await ApplyChangesToWeb(matchedChanges);
                    totalAppliedCount += appliedCount;
                    LogMessage($"페이지 {pageNumber}: {appliedCount}건 변경 완료");

                    // 변경사항 확인을 위해 2초 대기
                    LogMessage("⏸️ 변경사항 확인을 위해 2초 대기 중...");
                    await Task.Delay(2000, cancellationToken);
                }

                // 다음 페이지로 이동
                bool movedToNext = await MoveToNextPage();
                if (!movedToNext)
                {
                    LogMessage("다음 페이지가 없습니다. 모든 페이지 처리 완료.");
                    hasMorePages = false;
                }
                else
                {
                    LogMessage($"다음 페이지({pageNumber + 1})로 이동했습니다.");
                    await Task.Delay(2000, cancellationToken); // 페이지 로딩 대기
                }

                pageNumber++;
                progressBar.Value = Math.Min(60 + (pageNumber * 10), 100);
            }

            progressBar.Value = 100;
            LogMessage($"=== 모든 페이지 처리 완료 ===");
            LogMessage($"총 {totalMatchedCount}건 매칭, {totalAppliedCount}건 변경 적용");

            if (totalAppliedCount == 0)
            {
                LogMessage("변경할 항목이 없습니다. 모든 데이터가 이미 올바른 상태입니다.");
                MessageBox.Show("변경할 항목이 없습니다.", "완료", MessageBoxButtons.OK, MessageBoxIcon.Information);
                return;
            }

            // 변경 내역 상태 업데이트
            for (int i = 0; i < Math.Min(totalAppliedCount, changesTable.Rows.Count); i++)
            {
                changesTable.Rows[i]["상태"] = "완료";
            }

            LogMessage($"=== 자동 반영 완료: {totalAppliedCount}건 성공 ===");
            MessageBox.Show(
                $"총 {excelData.Count}건 중 {totalMatchedCount}건 매칭\n{totalAppliedCount}건의 공제여부가 변경되었습니다.\n\n변경 내역은 '변경 내역' 탭에서 확인하세요.",
                "완료",
                MessageBoxButtons.OK,
                MessageBoxIcon.Information
            );

            // 변경 내역 탭으로 전환
            tabControl.SelectedIndex = 1;
        }
        catch (OperationCanceledException)
        {
            progressBar.Value = 0;
            throw; // Re-throw to be handled by btnStartProcess_Click
        }
        catch (Exception ex)
        {
            LogMessage($"자동 반영 오류: {ex.Message}");
            MessageBox.Show($"자동 반영 중 오류가 발생했습니다.\n{ex.Message}", "오류", MessageBoxButtons.OK, MessageBoxIcon.Error);
            progressBar.Value = 0;
        }
    }

    private async Task<int> ApplyChangesToWeb(List<MatchedChange> changes)
    {
        LogMessage($"{changes.Count}개의 행에 변경사항을 적용합니다...");

        string script = _scriptGenerator.GenerateApplyChangesScript(changes);

        try
        {
            var result = await webView.CoreWebView2.ExecuteScriptAsync(script);

            // 디버그: 실제 반환값 로그
            LogMessage($"[디버그] JavaScript 반환값: {result?.Substring(0, Math.Min(200, result?.Length ?? 0))}...");

            // async function은 Promise를 반환하므로, 결과 형식이 다를 수 있음
            // 이중 JSON 문자열화 여부 확인
            string cleanJson;
            if (result?.StartsWith("\"") == true)
            {
                // 이중 JSON 문자열화된 경우
                cleanJson = JsonSerializer.Deserialize<string>(result);
            }
            else
            {
                // 직접 JSON 문자열인 경우
                cleanJson = result;
            }

            if (!string.IsNullOrEmpty(cleanJson))
            {
                var response = JsonSerializer.Deserialize<JsonElement>(cleanJson);

                if (response.TryGetProperty("error", out var errorProp))
                {
                    LogMessage($"❌ JavaScript 오류: {errorProp.GetString()}");
                    return 0;
                }

                if (response.TryGetProperty("successCount", out var successCountProp))
                {
                    int successCount = successCountProp.GetInt32();
                    LogMessage($"✅ {successCount}개의 행이 성공적으로 변경되었습니다.");

                    // 디버그 정보 로깅
                    if (response.TryGetProperty("debugInfo", out var debugInfoProp))
                    {
                        LogMessage("\n[디버그 정보]");
                        var debugArray = debugInfoProp.EnumerateArray().ToList();

                        for (int i = 0; i < debugArray.Count; i++)
                        {
                            var item = debugArray[i];

                            // 저장 버튼 정보인 경우
                            if (item.TryGetProperty("saveBtnFound", out var saveBtnFound))
                            {
                                LogMessage($"\n▶ 저장 버튼:");
                                LogMessage($"  - 발견됨: {saveBtnFound.GetBoolean()}");
                                if (item.TryGetProperty("saveBtnId", out var saveBtnId))
                                {
                                    LogMessage($"  - ID: {saveBtnId.GetString()}");
                                }
                                if (item.TryGetProperty("saveBtnText", out var saveBtnText))
                                {
                                    LogMessage($"  - 텍스트: {saveBtnText.GetString()}");
                                }
                                if (item.TryGetProperty("saveBtnClicked", out var saveBtnClicked))
                                {
                                    LogMessage($"  - 클릭됨: {saveBtnClicked.GetBoolean()}");
                                }
                                if (item.TryGetProperty("allButtonCount", out var allButtonCount))
                                {
                                    LogMessage($"  - 전체 버튼 개수: {allButtonCount.GetInt32()}");
                                }
                            }
                            // 행 정보인 경우
                            else if (item.TryGetProperty("rowIndex", out var rowIndex))
                            {
                                LogMessage($"\n▶ 행 {rowIndex.GetInt32()}:");

                                // 매칭된 데이터 정보 출력
                                if (item.TryGetProperty("aprvDt", out var aprvDt))
                                    LogMessage($"  - 날짜: {aprvDt.GetString()}");
                                if (item.TryGetProperty("bizNo", out var bizNo))
                                    LogMessage($"  - 사업자번호: {bizNo.GetString()}");
                                if (item.TryGetProperty("amount", out var amount))
                                    LogMessage($"  - 금액: {amount.GetDecimal():N0}");

                                if (item.TryGetProperty("targetValue", out var targetValue))
                                    LogMessage($"  - 목표값: {targetValue.GetString()}");

                                if (item.TryGetProperty("checkboxFound", out var chkFound))
                                {
                                    LogMessage($"  - 체크박스 발견: {chkFound.GetBoolean()}");
                                    if (item.TryGetProperty("checkboxId", out var chkId))
                                        LogMessage($"  - 체크박스 ID: {chkId.GetString()}");
                                }

                                if (item.TryGetProperty("selectFound", out var selFound))
                                {
                                    LogMessage($"  - Select 박스 발견: {selFound.GetBoolean()}");
                                    if (item.TryGetProperty("selectId", out var selId))
                                        LogMessage($"  - Select ID: {selId.GetString()}");
                                }
                            }
                        }
                        LogMessage("");
                    }

                    // 저장 버튼 클릭 (JavaScript에서 찾은 버튼 ID 사용)
                    if (response.TryGetProperty("debugInfo", out var debugInfoForSave))
                    {
                        var debugArray2 = debugInfoForSave.EnumerateArray().ToList();
                        foreach (var item in debugArray2)
                        {
                            if (item.TryGetProperty("saveBtnFound", out var found) && found.GetBoolean())
                            {
                                if (item.TryGetProperty("saveBtnId", out var btnId))
                                {
                                    string saveButtonId = btnId.GetString() ?? "";
                                    LogMessage($"저장 버튼 클릭 중... (ID: {saveButtonId})");
                                    await ClickSaveButtonById(saveButtonId);
                                }
                                break;
                            }
                        }
                    }

                    return successCount;
                }
            }
        }
        catch (Exception ex)
        {
            LogMessage($"변경 적용 오류: {ex.Message}");
        }

        return 0;
    }

    private async Task ClickSaveButtonById(string buttonId)
    {
        string script = $@"
(function() {{
    try {{
        var saveBtn = document.getElementById('{buttonId}');
        if (saveBtn) {{
            saveBtn.click();
            return JSON.stringify({{ success: true, message: '저장 버튼 클릭 완료' }});
        }} else {{
            return JSON.stringify({{ success: false, message: '저장 버튼을 찾을 수 없습니다' }});
        }}
    }} catch (ex) {{
        return JSON.stringify({{ success: false, message: ex.message }});
    }}
}})();
";

        try
        {
            var result = await webView.CoreWebView2.ExecuteScriptAsync(script);

            // JSON 파싱
            string cleanJson;
            if (result?.StartsWith("\"") == true)
            {
                cleanJson = JsonSerializer.Deserialize<string>(result);
            }
            else
            {
                cleanJson = result;
            }

            if (!string.IsNullOrEmpty(cleanJson))
            {
                var response = JsonSerializer.Deserialize<JsonElement>(cleanJson);
                if (response.TryGetProperty("success", out var successProp))
                {
                    bool success = successProp.GetBoolean();
                    if (response.TryGetProperty("message", out var messageProp))
                    {
                        LogMessage($"저장 결과: {messageProp.GetString()}");
                    }

                    if (success)
                    {
                        // 저장 처리 및 팝업 대기 (1초)
                        await Task.Delay(1000);

                        // "변경이 완료되었습니다" 팝업의 확인 버튼 클릭
                        await ClickConfirmPopup();

                        // 팝업 닫힌 후 추가 대기 (0.5초)
                        await Task.Delay(500);
                    }
                }
            }
        }
        catch (Exception ex)
        {
            LogMessage($"저장 버튼 클릭 오류: {ex.Message}");
        }
    }

    private async Task ClickConfirmPopup()
    {
        string script = @"
(function() {
    try {
        var debugInfo = [];
        var confirmBtn = null;

        // 패턴 1: ID로 찾기
        var btnPatterns = [
            'scwin_wfm_side_messageConfirm',
            'wfm_side_messageConfirm',
            'messageConfirm',
            'btnConfirm',
            'btnOk',
            'wfm_side_message',
            'wfm_layer'
        ];

        for (var i = 0; i < btnPatterns.length; i++) {
            var btn = document.getElementById(btnPatterns[i]);
            if (btn) {
                // 실제 마우스 클릭 이벤트 발생
                var clickEvent = new MouseEvent('click', {
                    view: window,
                    bubbles: true,
                    cancelable: true,
                    buttons: 1
                });
                btn.dispatchEvent(clickEvent);

                var mousedownEvent = new MouseEvent('mousedown', {
                    view: window,
                    bubbles: true,
                    cancelable: true,
                    buttons: 1
                });
                var mouseupEvent = new MouseEvent('mouseup', {
                    view: window,
                    bubbles: true,
                    cancelable: true,
                    buttons: 1
                });
                btn.dispatchEvent(mousedownEvent);
                btn.dispatchEvent(mouseupEvent);
                btn.click();

                return JSON.stringify({
                    success: true,
                    message: '확인 버튼 클릭 완료 (ID)',
                    buttonId: btnPatterns[i]
                });
            }
        }

        // 패턴 2: 모든 가시적인 버튼 찾기
        var allButtons = document.querySelectorAll('button, input[type=""button""], a, span, div');
        var visibleButtons = [];

        for (var i = 0; i < allButtons.length; i++) {
            var btn = allButtons[i];
            var style = window.getComputedStyle(btn);

            // 보이는 요소만 확인
            if (style.display !== 'none' && style.visibility !== 'hidden' && style.opacity !== '0') {
                var btnText = (btn.value || btn.textContent || btn.innerText || '').trim();

                if (btnText) {
                    visibleButtons.push({
                        tag: btn.tagName,
                        id: btn.id || '',
                        class: btn.className || '',
                        text: btnText.substring(0, 50),
                        display: style.display,
                        visibility: style.visibility
                    });

                    // '확인' 텍스트 찾기 (완전 일치 또는 포함)
                    if (btnText === '확인' || btnText === 'OK' || btnText === '닫기' ||
                        btnText.indexOf('확인') >= 0) {

                        // 실제 마우스 클릭 이벤트 발생
                        var clickEvent = new MouseEvent('click', {
                            view: window,
                            bubbles: true,
                            cancelable: true,
                            buttons: 1
                        });
                        btn.dispatchEvent(clickEvent);

                        // 추가로 mousedown, mouseup 이벤트도 발생
                        var mousedownEvent = new MouseEvent('mousedown', {
                            view: window,
                            bubbles: true,
                            cancelable: true,
                            buttons: 1
                        });
                        var mouseupEvent = new MouseEvent('mouseup', {
                            view: window,
                            bubbles: true,
                            cancelable: true,
                            buttons: 1
                        });
                        btn.dispatchEvent(mousedownEvent);
                        btn.dispatchEvent(mouseupEvent);

                        // 기본 click도 시도
                        btn.click();

                        return JSON.stringify({
                            success: true,
                            message: '확인 버튼 클릭 완료 (텍스트)',
                            buttonText: btnText,
                            buttonId: btn.id || '(no id)',
                            buttonClass: btn.className || '(no class)'
                        });
                    }
                }
            }
        }

        // 패턴 3: iframe 내부 확인
        var iframes = document.querySelectorAll('iframe');
        for (var i = 0; i < iframes.length; i++) {
            try {
                var iframeDoc = iframes[i].contentDocument || iframes[i].contentWindow.document;
                var iframeButtons = iframeDoc.querySelectorAll('button, input[type=""button""], a, span');

                for (var j = 0; j < iframeButtons.length; j++) {
                    var btn = iframeButtons[j];
                    var btnText = (btn.value || btn.textContent || '').trim();

                    if (btnText === '확인' || btnText === 'OK' || btnText === '닫기') {
                        // 실제 마우스 클릭 이벤트 발생
                        var clickEvent = new MouseEvent('click', {
                            view: iframeDoc.defaultView || window,
                            bubbles: true,
                            cancelable: true,
                            buttons: 1
                        });
                        btn.dispatchEvent(clickEvent);

                        var mousedownEvent = new MouseEvent('mousedown', {
                            view: iframeDoc.defaultView || window,
                            bubbles: true,
                            cancelable: true,
                            buttons: 1
                        });
                        var mouseupEvent = new MouseEvent('mouseup', {
                            view: iframeDoc.defaultView || window,
                            bubbles: true,
                            cancelable: true,
                            buttons: 1
                        });
                        btn.dispatchEvent(mousedownEvent);
                        btn.dispatchEvent(mouseupEvent);
                        btn.click();

                        return JSON.stringify({
                            success: true,
                            message: '확인 버튼 클릭 완료 (iframe)',
                            buttonText: btnText
                        });
                    }
                }
            } catch (e) {
                // iframe 접근 불가 (CORS)
            }
        }

        return JSON.stringify({
            success: false,
            message: '확인 버튼을 찾을 수 없습니다',
            totalButtons: allButtons.length,
            visibleButtons: visibleButtons.slice(0, 20)
        });
    } catch (ex) {
        return JSON.stringify({ success: false, message: ex.message });
    }
})();
";

        try
        {
            var result = await webView.CoreWebView2.ExecuteScriptAsync(script);

            // JSON 파싱
            string cleanJson;
            if (result?.StartsWith("\"") == true)
            {
                cleanJson = JsonSerializer.Deserialize<string>(result);
            }
            else
            {
                cleanJson = result;
            }

            if (!string.IsNullOrEmpty(cleanJson))
            {
                var response = JsonSerializer.Deserialize<JsonElement>(cleanJson);
                if (response.TryGetProperty("success", out var successProp))
                {
                    bool success = successProp.GetBoolean();
                    if (response.TryGetProperty("message", out var messageProp))
                    {
                        LogMessage($"팝업 처리: {messageProp.GetString()}");
                    }

                    // 실패한 경우 디버그 정보 출력
                    if (!success && response.TryGetProperty("visibleButtons", out var visibleButtonsProp))
                    {
                        LogMessage("\n[발견된 버튼 목록]");
                        var buttons = visibleButtonsProp.EnumerateArray().ToList();
                        for (int i = 0; i < Math.Min(buttons.Count, 10); i++)
                        {
                            var btn = buttons[i];
                            if (btn.TryGetProperty("text", out var text) &&
                                btn.TryGetProperty("tag", out var tag))
                            {
                                var id = btn.TryGetProperty("id", out var idProp) ? idProp.GetString() : "";
                                var className = btn.TryGetProperty("class", out var classProp) ? classProp.GetString() : "";
                                LogMessage($"  {i + 1}. <{tag.GetString()}> id=\"{id}\" class=\"{className}\" → {text.GetString()}");
                            }
                        }

                        if (response.TryGetProperty("totalButtons", out var totalProp))
                        {
                            LogMessage($"\n총 버튼 개수: {totalProp.GetInt32()}");
                        }
                    }
                }
            }
        }
        catch (Exception ex)
        {
            LogMessage($"팝업 클릭 오류: {ex.Message}");
        }
    }

    private async Task ClickSaveButton()
    {
        string script = @"
(function() {
    try {
        var debugInfo = [];

        // 저장 버튼 찾기 - 여러 패턴 시도
        var saveBtnPatterns = [
            'mf_txppWframe_btnReg',
            'btnSave',
            'btnReg',
            'btnRegister',
            'mf_txppWframe_btnSave'
        ];

        var saveBtn = null;
        for (var i = 0; i < saveBtnPatterns.length; i++) {
            var btn = document.getElementById(saveBtnPatterns[i]);
            if (btn) {
                saveBtn = btn;
                debugInfo.push('ID로 찾음: ' + saveBtnPatterns[i]);
                break;
            }
        }

        // ID로 못 찾으면 텍스트로 찾기
        if (!saveBtn) {
            var buttons = document.querySelectorAll('button, a, span, input[type=""button""]');
            debugInfo.push('전체 버튼 개수: ' + buttons.length);

            // 모든 버튼의 정보 수집 (처음 20개만)
            var buttonInfos = [];
            for (var i = 0; i < Math.min(20, buttons.length); i++) {
                var btn = buttons[i];
                var btnText = btn.textContent.trim();
                var btnId = btn.id || '(no id)';
                var btnClass = btn.className || '(no class)';

                buttonInfos.push({
                    index: i,
                    id: btnId,
                    text: btnText.substring(0, 30),
                    class: btnClass.substring(0, 50)
                });

                if (btnText === '저장' || btnText === '등록' || btnText === 'Save' ||
                    btnText.indexOf('저장') >= 0 || btnText.indexOf('등록') >= 0) {
                    saveBtn = btn;
                    debugInfo.push('텍스트로 찾음: ' + btnText + ' (ID: ' + btnId + ')');
                    break;
                }
            }

            debugInfo.push('버튼 목록: ' + JSON.stringify(buttonInfos));
        }

        if (saveBtn) {
            saveBtn.click();
            return JSON.stringify({
                success: true,
                message: '저장 버튼 클릭 완료',
                debugInfo: debugInfo
            });
        } else {
            return JSON.stringify({
                success: false,
                message: '저장 버튼을 찾을 수 없습니다',
                debugInfo: debugInfo
            });
        }
    } catch (ex) {
        return JSON.stringify({
            success: false,
            message: ex.message,
            debugInfo: debugInfo || []
        });
    }
})();
";

        try
        {
            var result = await webView.CoreWebView2.ExecuteScriptAsync(script);

            // JSON 파싱
            string cleanJson;
            if (result?.StartsWith("\"") == true)
            {
                cleanJson = JsonSerializer.Deserialize<string>(result);
            }
            else
            {
                cleanJson = result;
            }

            if (!string.IsNullOrEmpty(cleanJson))
            {
                var response = JsonSerializer.Deserialize<JsonElement>(cleanJson);
                if (response.TryGetProperty("success", out var successProp))
                {
                    bool success = successProp.GetBoolean();
                    if (response.TryGetProperty("message", out var messageProp))
                    {
                        LogMessage($"저장 버튼: {messageProp.GetString()}");
                    }

                    // 디버그 정보 출력
                    if (response.TryGetProperty("debugInfo", out var debugInfoProp) && chkDetailedLog.Checked)
                    {
                        LogMessage("[저장 버튼 디버그 정보]");
                        foreach (var info in debugInfoProp.EnumerateArray())
                        {
                            LogMessage($"  {info.GetString()}");
                        }
                    }

                    if (success)
                    {
                        // 저장 처리를 위해 1초 대기
                        await Task.Delay(1000);
                    }
                }
            }
        }
        catch (Exception ex)
        {
            LogMessage($"저장 버튼 클릭 오류: {ex.Message}");
        }
    }

    private async Task<List<WebTableRow>> ExtractWebTableData()
    {
        LogMessage("웹 테이블 데이터 추출 중...");

        string script = _scriptGenerator.GenerateTableExtractionScript();

        try
        {
            var jsonResult = await webView.CoreWebView2.ExecuteScriptAsync(script);
            var cleanJson = JsonSerializer.Deserialize<string>(jsonResult);

            if (cleanJson != null)
            {
                var options = new JsonSerializerOptions
                {
                    PropertyNameCaseInsensitive = true
                };
                var extractionResult = JsonSerializer.Deserialize<TableExtractionResult>(cleanJson, options);

                if (extractionResult != null && extractionResult.success)
                {
                    LogMessage($"웹 테이블에서 {extractionResult.rows.Count}개의 행을 추출했습니다.");

                    // 상세 로그 모드일 때만 전체 데이터 출력
                    if (chkDetailedLog.Checked)
                    {
                        LogMessage("\n[추출된 웹 테이블 데이터]");
                        foreach (var row in extractionResult.rows)
                        {
                            LogMessage($"  행 {row.RowIndex}: {row.AprvDt} | {row.MrntTxprDscmNoEncCntn} | {row.MrntNm} | {row.TotaTrsAmt:N0}원 | {row.CurrentDdcYnNm}");
                        }
                        LogMessage("");
                    }

                    return extractionResult.rows;
                }
                else
                {
                    LogMessage($"웹 테이블 추출 실패: {extractionResult?.error}");
                }
            }
        }
        catch (Exception ex)
        {
            LogMessage($"웹 테이블 추출 오류: {ex.Message}");
        }

        return new List<WebTableRow>();
    }

    private async Task<bool> MoveToNextPage()
    {
        try
        {
            string script = @"
(function() {
    try {
        // 다음 페이지 버튼 찾기 - 여러 패턴 시도
        var nextBtnPatterns = [
            'mf_txppWframe_grdCshpt_nextPageBtn',
            'btnNextPage',
            'btnNext',
            'pageNext'
        ];

        var nextBtn = null;
        for (var i = 0; i < nextBtnPatterns.length; i++) {
            nextBtn = document.getElementById(nextBtnPatterns[i]);
            if (nextBtn) break;
        }

        // ID로 못 찾으면 텍스트로 찾기
        if (!nextBtn) {
            var buttons = document.querySelectorAll('button, a, span');
            for (var i = 0; i < buttons.length; i++) {
                var btnText = buttons[i].textContent.trim();
                var title = buttons[i].getAttribute('title') || '';
                if (btnText === '다음' || btnText === 'Next' || btnText === '>' ||
                    title.indexOf('다음') >= 0 || title.indexOf('Next') >= 0) {
                    nextBtn = buttons[i];
                    break;
                }
            }
        }

        if (!nextBtn) {
            return JSON.stringify({ success: false, message: '다음 페이지 버튼을 찾을 수 없습니다' });
        }

        // 버튼이 비활성화되어 있는지 확인
        if (nextBtn.disabled || nextBtn.classList.contains('disabled') ||
            nextBtn.getAttribute('aria-disabled') === 'true') {
            return JSON.stringify({ success: false, message: '다음 페이지가 없습니다 (버튼 비활성화)' });
        }

        // 버튼 클릭
        nextBtn.click();
        return JSON.stringify({ success: true, message: '다음 페이지로 이동' });

    } catch (ex) {
        return JSON.stringify({ success: false, message: ex.message });
    }
})();
";

            var result = await webView.CoreWebView2.ExecuteScriptAsync(script);

            // JSON 파싱
            string cleanJson;
            if (result?.StartsWith("\"") == true)
            {
                cleanJson = JsonSerializer.Deserialize<string>(result);
            }
            else
            {
                cleanJson = result;
            }

            if (!string.IsNullOrEmpty(cleanJson))
            {
                var response = JsonSerializer.Deserialize<JsonElement>(cleanJson);
                if (response.TryGetProperty("success", out var successProp))
                {
                    bool success = successProp.GetBoolean();
                    if (response.TryGetProperty("message", out var messageProp))
                    {
                        LogMessage($"페이지 이동: {messageProp.GetString()}");
                    }
                    return success;
                }
            }
        }
        catch (Exception ex)
        {
            LogMessage($"페이지 이동 오류: {ex.Message}");
        }

        return false;
    }

    private (int year, int quarter) GetDateRangeFromExcelData(List<CardTransactionData> excelData)
    {
        LogMessage($"엑셀 데이터에서 날짜 범위 추출 중... (총 {excelData.Count}건)");

        // 처음 몇 개의 날짜 문자열 로그
        for (int i = 0; i < Math.Min(5, excelData.Count); i++)
        {
            LogMessage($"  샘플 {i + 1}: 승인일자 = '{excelData[i].승인일자}'");
        }

        // 엑셀 데이터에서 유효한 날짜들 파싱
        var parsedDates = excelData
            .Select((d, index) => {
                if (DateTime.TryParse(d.승인일자, out DateTime date))
                {
                    if (index < 5) LogMessage($"  파싱 성공 {index + 1}: '{d.승인일자}' -> {date:yyyy-MM-dd} (월: {date.Month})");
                    return (DateTime?)date;
                }
                else
                {
                    if (index < 5) LogMessage($"  파싱 실패 {index + 1}: '{d.승인일자}'");
                }
                return null;
            })
            .Where(d => d.HasValue)
            .Select(d => d!.Value)
            .ToList();

        if (parsedDates.Count == 0)
        {
            LogMessage("⚠️ 유효한 날짜를 찾을 수 없습니다. 현재 날짜 사용.");
            var now = DateTime.Now;
            var nowQuarter = (now.Month - 1) / 3 + 1;
            LogMessage($"  현재: {now:yyyy-MM-dd} -> {now.Year}년 {nowQuarter}분기");
            return (now.Year, nowQuarter);
        }

        // 최소 날짜와 최대 날짜 찾기
        var minDate = parsedDates.Min();
        var maxDate = parsedDates.Max();

        LogMessage($"✓ 데이터 날짜 범위: {minDate:yyyy-MM-dd} ~ {maxDate:yyyy-MM-dd} (총 {parsedDates.Count}개 날짜)");

        // 최대 날짜(가장 최근 날짜)를 기준으로 분기 결정
        // 이유: 분기 데이터에 이전 분기 마지막 며칠이 포함될 수 있으므로,
        // 대부분의 데이터가 속한 분기는 최근 날짜 기준이 더 정확함
        int year = maxDate.Year;
        int month = maxDate.Month;
        int quarter = (month - 1) / 3 + 1;

        var minQuarter = (minDate.Month - 1) / 3 + 1;
        if (minDate.Year == year && minQuarter != quarter)
        {
            LogMessage($"  ℹ️ 시작날({minDate:MM-dd})은 {minQuarter}분기, 마지막날({maxDate:MM-dd})은 {quarter}분기");
            LogMessage($"  -> 대부분의 데이터가 속한 {quarter}분기를 선택합니다");
        }

        LogMessage($"✓ 계산: 최대날짜 월={month} -> (({month}-1)/3)+1 = {quarter}분기");
        LogMessage($"✓ 결과: {year}년 {quarter}분기");

        return (year, quarter);
    }

    private async Task<bool> GoToNextPage()
    {
        string script = @"
            (function() {
                try {
                    // 현재 페이지가 마지막 페이지인지 확인
                    var paginationDiv = document.getElementById('mf_txppWframe_pglNavi');
                    if (!paginationDiv) {
                        return 'ERROR: 페이지네이션을 찾을 수 없습니다.';
                    }

                    // 현재 active 페이지 찾기
                    var activeLink = paginationDiv.querySelector('a.active');
                    if (!activeLink) {
                        return 'ERROR: 현재 페이지를 찾을 수 없습니다.';
                    }

                    var currentPage = parseInt(activeLink.getAttribute('data-index'));

                    // 모든 페이지 링크 찾기
                    var allPageLinks = paginationDiv.querySelectorAll('a.w2page_link');
                    var maxPage = 0;
                    for (var i = 0; i < allPageLinks.length; i++) {
                        var pageIndex = parseInt(allPageLinks[i].getAttribute('data-index'));
                        if (pageIndex > maxPage) {
                            maxPage = pageIndex;
                        }
                    }

                    // 마지막 페이지인지 확인
                    if (currentPage >= maxPage) {
                        return 'LAST_PAGE';
                    }

                    // 다음 버튼 클릭
                    var nextBtn = document.getElementById('mf_txppWframe_pglNavi_nextPage_btn');
                    if (!nextBtn) {
                        return 'ERROR: 다음 버튼을 찾을 수 없습니다.';
                    }

                    nextBtn.click();
                    return 'OK:' + (currentPage + 1);
                } catch (ex) {
                    return 'ERROR: ' + ex.message;
                }
            })();
        ";

        try
        {
            var result = await webView.CoreWebView2.ExecuteScriptAsync(script);
            var cleanResult = result?.Trim('"') ?? "";

            LogMessage($"다음 페이지 이동: {cleanResult}");

            if (cleanResult == "LAST_PAGE")
            {
                LogMessage("마지막 페이지에 도달했습니다.");
                return false;
            }
            else if (cleanResult.StartsWith("OK:"))
            {
                return true;
            }
            else
            {
                LogMessage($"페이지 이동 실패: {cleanResult}");
                return false;
            }
        }
        catch (Exception ex)
        {
            LogMessage($"페이지 이동 오류: {ex.Message}");
            return false;
        }
    }

    private async Task SetSearchPeriodAndSearch(int year, int quarter)
    {
        LogMessage($"조회 기간을 {year}년 {quarter}분기로 설정합니다...");

        // 1단계: 분기별 라디오 버튼 선택
        string selectQuarterlyScript = @"
            (function() {
                try {
                    var quarterlyRadio = document.getElementById('mf_txppWframe_rdoSearch_input_2');
                    if (!quarterlyRadio) {
                        return 'ERROR: 분기별 라디오 버튼을 찾을 수 없습니다.';
                    }

                    quarterlyRadio.checked = true;
                    quarterlyRadio.click();

                    // 드롭다운 컨테이너 강제로 표시
                    var dropdownContainer = document.getElementById('mf_txppWframe_group2185');
                    if (dropdownContainer) {
                        dropdownContainer.style.display = '';
                    }

                    return 'OK';
                } catch (ex) {
                    return 'ERROR: ' + ex.message;
                }
            })();
        ";

        var result1 = await webView.CoreWebView2.ExecuteScriptAsync(selectQuarterlyScript);
        LogMessage($"분기별 선택 결과: {result1}");
        await Task.Delay(1000); // 1초 대기

        // 2단계: 년도 및 분기 선택
        string selectPeriodScript = $@"
            (function() {{
                try {{
                    var yearSelect = document.getElementById('mf_txppWframe_selectYear');
                    var quarterSelect = document.getElementById('mf_txppWframe_selectQrt');

                    if (!yearSelect || !quarterSelect) {{
                        return 'ERROR: 년도 또는 분기 드롭다운을 찾을 수 없습니다.';
                    }}

                    // 년도 선택
                    for (var i = 0; i < yearSelect.options.length; i++) {{
                        if (yearSelect.options[i].text.includes('{year}')) {{
                            yearSelect.selectedIndex = i;
                            yearSelect.dispatchEvent(new Event('change', {{ bubbles: true }}));
                            break;
                        }}
                    }}

                    // 분기 선택
                    quarterSelect.selectedIndex = {quarter - 1};
                    quarterSelect.dispatchEvent(new Event('change', {{ bubbles: true }}));

                    return 'OK: ' + {year} + '년 ' + {quarter} + '분기 선택됨';
                }} catch (ex) {{
                    return 'ERROR: ' + ex.message;
                }}
            }})();
        ";

        var result2 = await webView.CoreWebView2.ExecuteScriptAsync(selectPeriodScript);
        LogMessage($"기간 선택 결과: {result2}");
        await Task.Delay(500);

        // 3단계: 조회 버튼 클릭
        string clickSearchScript = @"
            (function() {
                try {
                    var searchBtn = document.getElementById('mf_txppWframe_btnSearch');
                    if (!searchBtn) {
                        return 'ERROR: 조회 버튼을 찾을 수 없습니다.';
                    }

                    searchBtn.click();
                    return 'OK: 조회 버튼 클릭됨';
                } catch (ex) {
                    return 'ERROR: ' + ex.message;
                }
            })();
        ";

        var result3 = await webView.CoreWebView2.ExecuteScriptAsync(clickSearchScript);
        LogMessage($"조회 버튼 클릭 결과: {result3}");
    }

    private async Task NavigateToCardMenu()
    {
        LogMessage("메뉴 자동 진입을 시작합니다...");

        string script = @"
            (function() {
                try {
                    var result = {
                        success: false,
                        currentStep: '',
                        error: null
                    };

                    // 헬퍼 함수: 텍스트로 메뉴 찾기 (span 내부 텍스트 확인 후 부모 a 태그 찾기)
                    function findMenuByText(searchText) {
                        var allElements = document.querySelectorAll('span, a');
                        for (var i = 0; i < allElements.length; i++) {
                            var text = allElements[i].textContent.trim();
                            if (text === searchText || text.includes(searchText)) {
                                // span이면 부모 a 태그 찾기
                                if (allElements[i].tagName === 'SPAN') {
                                    var parent = allElements[i].parentElement;
                                    if (parent && parent.tagName === 'A') {
                                        return parent;
                                    }
                                } else if (allElements[i].tagName === 'A') {
                                    return allElements[i];
                                }
                            }
                        }
                        return null;
                    }

                    // 헬퍼 함수: 메뉴 클릭 (onclick 핸들러 실행)
                    function clickMenu(menuElement) {
                        if (!menuElement) return false;

                        // onclick 속성이 있으면 eval로 실행
                        var onclickAttr = menuElement.getAttribute('onclick');
                        if (onclickAttr) {
                            try {
                                eval(onclickAttr);
                                return true;
                            } catch (e) {
                                // eval 실패시 일반 클릭 시도
                            }
                        }

                        // 일반 클릭
                        menuElement.click();
                        return true;
                    }

                    // 1. '계산서·영수증·카드' 메뉴 찾기 및 클릭
                    result.currentStep = '계산서·영수증·카드 메뉴 찾는 중';
                    var mainMenu = findMenuByText('계산서·영수증·카드');

                    if (!mainMenu) {
                        result.error = '계산서·영수증·카드 메뉴를 찾을 수 없습니다.';
                        window.chrome.webview.postMessage(JSON.stringify(result));
                        return;
                    }

                    result.currentStep = '계산서·영수증·카드 메뉴 클릭';
                    clickMenu(mainMenu);

                    // 잠시 대기 후 다음 메뉴 클릭 시도
                    setTimeout(function() {
                        // 2. '신용카드 매입' 메뉴 찾기
                        result.currentStep = '신용카드 매입 메뉴 찾는 중';
                        var cardMenu = findMenuByText('신용카드 매입');

                        if (!cardMenu) {
                            result.error = '신용카드 매입 메뉴를 찾을 수 없습니다.';
                            window.chrome.webview.postMessage(JSON.stringify(result));
                            return;
                        }

                        result.currentStep = '신용카드 매입 메뉴 클릭';
                        clickMenu(cardMenu);

                        // 다시 대기 후 다음 메뉴
                        setTimeout(function() {
                            // 3. '사업용 신용카드 사용내역' 메뉴 찾기
                            result.currentStep = '사업용 신용카드 사용내역 메뉴 찾는 중';
                            var businessCardMenu = findMenuByText('사업용 신용카드 사용내역');

                            if (!businessCardMenu) {
                                result.error = '사업용 신용카드 사용내역 메뉴를 찾을 수 없습니다.';
                                window.chrome.webview.postMessage(JSON.stringify(result));
                                return;
                            }

                            result.currentStep = '사업용 신용카드 사용내역 메뉴 클릭';
                            clickMenu(businessCardMenu);

                            // 마지막 메뉴
                            setTimeout(function() {
                                // 4. '매입세액 공제 확인/변경' 메뉴 찾기
                                result.currentStep = '매입세액 공제 확인/변경 메뉴 찾는 중';
                                var confirmMenu = findMenuByText('매입세액 공제 확인/변경');

                                if (!confirmMenu) {
                                    result.error = '매입세액 공제 확인/변경 메뉴를 찾을 수 없습니다.';
                                    window.chrome.webview.postMessage(JSON.stringify(result));
                                    return;
                                }

                                result.currentStep = '매입세액 공제 확인/변경 메뉴 클릭';
                                clickMenu(confirmMenu);

                                result.success = true;
                                result.currentStep = '메뉴 진입 완료';
                                window.chrome.webview.postMessage(JSON.stringify(result));
                            }, 1000);
                        }, 1000);
                    }, 1000);

                } catch (ex) {
                    var errorResult = {
                        success: false,
                        currentStep: result.currentStep || '알 수 없음',
                        error: ex.message
                    };
                    window.chrome.webview.postMessage(JSON.stringify(errorResult));
                }
            })();
        ";

        await webView.CoreWebView2.ExecuteScriptAsync(script);
    }

    private async void btnExtractHTML_Click(object sender, EventArgs e)
    {
        if (!isWebViewInitialized)
        {
            LogMessage("WebView2가 아직 초기화되지 않았습니다.");
            return;
        }

        try
        {
            LogMessage("HTML 추출 중...");

            string script = "document.documentElement.outerHTML;";
            string htmlContent = await webView.CoreWebView2.ExecuteScriptAsync(script);

            // JSON 이스케이프 제거
            htmlContent = System.Text.Json.JsonSerializer.Deserialize<string>(htmlContent) ?? "";

            // 파일 저장
            string timestamp = DateTime.Now.ToString("yyyyMMdd_HHmmss");
            string fileName = $"hometax_page_{timestamp}.html";
            string filePath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.Desktop), fileName);

            File.WriteAllText(filePath, htmlContent, System.Text.Encoding.UTF8);

            LogMessage($"HTML 파일 저장 완료: {filePath}");
            MessageBox.Show($"HTML이 저장되었습니다.\n\n경로: {filePath}", "완료", MessageBoxButtons.OK, MessageBoxIcon.Information);
        }
        catch (Exception ex)
        {
            LogMessage($"HTML 추출 오류: {ex.Message}");
        }
    }

    private void btnDevTools_Click(object sender, EventArgs e)
    {
        if (!isWebViewInitialized)
        {
            LogMessage("WebView2가 아직 초기화되지 않았습니다.");
            return;
        }

        try
        {
            webView.CoreWebView2.OpenDevToolsWindow();
            LogMessage("개발자 도구를 열었습니다.");
        }
        catch (Exception ex)
        {
            LogMessage($"개발자 도구 열기 오류: {ex.Message}");
        }
    }

    private void btnDownloadExcel_Click(object sender, EventArgs e)
    {
        if (changesTable.Rows.Count == 0)
        {
            MessageBox.Show("다운로드할 변경 내역이 없습니다.", "알림", MessageBoxButtons.OK, MessageBoxIcon.Information);
            return;
        }

        using (SaveFileDialog saveFileDialog = new SaveFileDialog())
        {
            saveFileDialog.Filter = "CSV Files|*.csv|All Files|*.*";
            saveFileDialog.Title = "변경 내역 저장";
            saveFileDialog.FileName = $"변경내역_{DateTime.Now:yyyyMMdd_HHmmss}.csv";

            if (saveFileDialog.ShowDialog() == DialogResult.OK)
            {
                try
                {
                    ExportDataTableToCsv(changesTable, saveFileDialog.FileName);
                    LogMessage($"변경 내역 저장 완료: {saveFileDialog.FileName}");
                    MessageBox.Show("변경 내역이 저장되었습니다.", "완료", MessageBoxButtons.OK, MessageBoxIcon.Information);
                }
                catch (Exception ex)
                {
                    LogMessage($"파일 저장 오류: {ex.Message}");
                    MessageBox.Show($"파일 저장 중 오류가 발생했습니다.\n{ex.Message}", "오류", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            }
        }
    }

    private void ExportDataTableToCsv(DataTable dt, string filePath)
    {
        using (StreamWriter sw = new StreamWriter(filePath, false, System.Text.Encoding.UTF8))
        {
            // 헤더 작성
            for (int i = 0; i < dt.Columns.Count; i++)
            {
                sw.Write(dt.Columns[i].ColumnName);
                if (i < dt.Columns.Count - 1)
                    sw.Write(",");
            }
            sw.WriteLine();

            // 데이터 작성
            foreach (DataRow row in dt.Rows)
            {
                for (int i = 0; i < dt.Columns.Count; i++)
                {
                    string value = row[i].ToString() ?? "";
                    // CSV 이스케이프 처리
                    if (value.Contains(",") || value.Contains("\"") || value.Contains("\n"))
                    {
                        value = "\"" + value.Replace("\"", "\"\"") + "\"";
                    }
                    sw.Write(value);
                    if (i < dt.Columns.Count - 1)
                        sw.Write(",");
                }
                sw.WriteLine();
            }
        }
    }

    private void LogMessage(string message)
    {
        if (txtLog.InvokeRequired)
        {
            txtLog.Invoke(() => LogMessage(message));
        }
        else
        {
            string prefix = chkDetailedLog.Checked ? "[상세] " : "";
            txtLog.AppendText($"{prefix}[{DateTime.Now:HH:mm:ss}] {message}{Environment.NewLine}");
            txtLog.SelectionStart = txtLog.Text.Length;
            txtLog.ScrollToCaret();
        }
    }

    private class DOMTestResult
    {
        public bool success { get; set; }
        public int rowCount { get; set; }
        public string firstRowText { get; set; } = "";
        public bool modificationSuccess { get; set; }
        public string? error { get; set; }
    }

    private class NavigationResult
    {
        public bool success { get; set; }
        public string currentStep { get; set; } = "";
        public string? error { get; set; }
    }

    private class TableExtractionResult
    {
        public bool success { get; set; }
        public List<WebTableRow> rows { get; set; } = new List<WebTableRow>();
        public string? error { get; set; }
    }
}
