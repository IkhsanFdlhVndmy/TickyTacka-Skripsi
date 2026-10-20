using System.Collections.Generic;
using System.Threading;
using Audio;
using Cysharp.Threading.Tasks;
using Input;
using Manager;
using Player;
using TilemapLayer;
using Turnbase_System;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.SceneManagement;

public enum BattleResult
{
    PlayerWin,
    EnemyWin
}

public class TurnBaseSystem : MonoBehaviour
{
    public static TurnBaseSystem Instance;
    [SerializeField] private UIManagerBattle _uIManagerBattle;

    [Header("TilemapLayer")]
    [SerializeField] private BattleAreaTilemap _battleArea;
    [SerializeField] private BattleBoardTilemap _battleBoard;
    [SerializeField] private PreviewTilemap _previewTilemap;

    [Header("Enemy")]
    [SerializeField] private List<UnitData> _enemies;
    public UIManagerBattle UIManagerBattle => _uIManagerBattle;
    public UnitModel ActiveUnit => _activeUnit;
    private int _maxPlayer;
    private Dictionary<UnitData,CardSO> _playerCards;
    private List<UnitData> _players;
    public FiniteStateMachine<BattleState> BattleState { get; private set; }
    public PlayerTurnState PlayerTurnState { get; private set; }
    public SelectCardState SelectCardState { get; private set; }
    public EnemyTurnState EnemyTurnState { get; private set; }
    public GameEndState GameEndState { get; private set; }
    public BattleResult BattleResult { get; private set; }
    private UnitModel _activeUnit;
    private CancellationTokenSource _cts;
    private CancellationToken _cancellationToken;
    private Vector2 _mousePos;
    private Camera _mainCamera;
    private Vector3 _pendingMove;
    private bool _confirmMove;
    private AudioManager _audioManager;
    private void Awake()
    {
        _players = new List<UnitData>();
        _playerCards = new Dictionary<UnitData, CardSO>();
        _mainCamera = Camera.main;

        if (Instance == null) Instance = this;
        else Destroy(this);
    }
    private void Start()
    {
        _audioManager = AudioManager.Instance;
        SelectCardState = new SelectCardState(this);
        PlayerTurnState = new PlayerTurnState(this);
        EnemyTurnState = new EnemyTurnState(this);
        GameEndState = new GameEndState(this);
        Initialize();
    }

    public void StartGame()
    {
        BattleState = new FiniteStateMachine<BattleState>(SelectCardState);
        UIManagerBattle.HideTutorial();
        InitializePlayerLoc();
        InitializeEnemy();
    }
    private void Initialize()
    {
        Scene scene = SceneManager.GetActiveScene();
        if (scene.name == "Stage1") UIManagerBattle.ShowTutorial();
        else
        {
            BattleState = new FiniteStateMachine<BattleState>(SelectCardState);
            InitializePlayerLoc();
            InitializeEnemy();
        }
    }

    private void OnEnable()
    {
        InputManager.Instance.PlayerInput.Performed.OnDown += OnLeftMouseClicked;
    }

    private void OnDisable()
    {
        InputManager.Instance.PlayerInput.Performed.OnDown -= OnLeftMouseClicked;
    }

    public void SetBattleResult(BattleResult result)
    {
        BattleResult = result;
    }

    public void OnVictoryPlayer()
    {
        UIManagerBattle.ShowRecruitCards();
        UIManagerBattle.HideVictoryPanel();
    }
    public void SetPlayer(CardSO cardSo)
    {
        _playerCards[cardSo.UnitData] = cardSo;
        _players.Add(cardSo.UnitData);
    }

    public CardSO GetPlayerCard(UnitData unitData)
    {
        _playerCards.TryGetValue(unitData, out var dCardSo);
        return dCardSo;
    }
    public void OnDoneSelectPlayer()
    {
        CompleteSelectCard().Forget();
    }

    private async UniTask CompleteSelectCard()
    {
        Debug.Log("COMPLETE");
        InitializePlayer();
        await UIManagerBattle.ShowAnnouncement("BATTLE START");
        BattleState.ChangeState(PlayerTurnState);
        _battleBoard.HideTileView();
    }
    private void InitializePlayerLoc()
    {
        var playerSpawns = _battleBoard.GetSpawnLoc(UnitSide.Player);   // Sorted by key
        _maxPlayer = playerSpawns.Count + 1;
    }
    private void InitializePlayer()
    {
        Debug.Log("INITIALIZE");
        var playerSpawns = _battleBoard.GetSpawnLoc(UnitSide.Player);   // Sorted by key
        int i = 0;
        int count = 0;
        foreach (var kv in playerSpawns)
        {
            count++;
            if (count <= _players.Count)
            {
                _battleBoard.Build(kv.Value, _players[i++].UnitPrefab, _players[i - 1]);
            }
        }
    }
    private void InitializeEnemy()
    {
        var enemySpawns = _battleBoard.GetSpawnLoc(UnitSide.Enemy);   // Sorted by key
        int i = 0;
        foreach (var kv in enemySpawns)
        {
            _battleBoard.Build(kv.Value, _enemies[i++].UnitPrefab, _enemies[i - 1]);
        }
    }
    public void ShowPlayerMove(UnitModel unitModel)
    {
        _battleArea.ShowMoveTile(unitModel);
    }

    public void HidePlayerMove()
    {
        _battleArea.HideMoveTile();
    }

    public void ShowPlayerAttack(UnitModel unitModel)
    {
        _battleArea.ShowAttackTile(unitModel);
    }

    public void HidePlayerAttack()
    {
        _battleArea.HideAttackTile();
    }
    public void ShowPreview(Vector3 position)
        => _previewTilemap.ShowPreview(
            _activeUnit,
            position,
            IsValid(position)
        );
    public void ClearPreview()
        => _previewTilemap.ClearPreview();

    public bool IsValid(Vector3 worldPosition)
        => _battleBoard.IsEmpty(worldPosition) && _battleArea.IsValidMoveCell(worldPosition);
    public void StartPreview()
    {
        _cts = new CancellationTokenSource();
        _cancellationToken = _cts.Token;
        PreviewLoopAsync(_cancellationToken).Forget();
    }

    public void StopPreview()
    {
        if (_cts != null)
        {
            _cts.Cancel();
            _cts.Dispose();
            _cts = null;
        }
        ClearPreview();
    }

    public void FreezePreview()
    {
        if (_cts != null)
        {
            _cts.Cancel();
            _cts.Dispose();
            _cts = null;
        }
    }
    public async UniTaskVoid PreviewLoopAsync(CancellationToken token)
    {
        while (!token.IsCancellationRequested)
        {
            _mousePos = InputManager.Instance.PlayerInput.MousePos.Get();
            var worldPosition = _mainCamera.ScreenToWorldPoint(_mousePos);
            ShowPreview(worldPosition);
            await UniTask.Yield(token);
        }
    }
    public void SetActiveUnit(UnitModel unitModel)
    {
        _activeUnit = unitModel;
    }
    public bool IsPointerOverUI(Vector2 screenPosition)
    {
        var eventData = new PointerEventData(EventSystem.current)
        {
            position = screenPosition
        };
        var results = new List<RaycastResult>();
        EventSystem.current.RaycastAll(eventData, results);
        return results.Count > 0;
    }
    private void OnLeftMouseClicked()
    {

        _mousePos = InputManager.Instance.PlayerInput.MousePos.Get();
        if (IsPointerOverUI(_mousePos)) return;
        if (BattleState.CurrentState != PlayerTurnState) return;
        var worldPosition = _mainCamera.ScreenToWorldPoint(_mousePos);
        UnitModel item = _battleBoard.GetUnit(worldPosition);
        if (item != null && item.UnitData)
        {
            var isPlayer = item?.UnitData.UnitSide == UnitSide.Player;
            var unitController = _uIManagerBattle.UnitController;
            if (isPlayer && !unitController.AlreadyMove && !unitController.OnMoveUnit)
            {
                _audioManager.PlaySound(SoundType.SFX_TileChoose);
                _uIManagerBattle.ShowUnitAction(item, item.WorldCoords);
            }
        }
        if (_activeUnit != null && !_confirmMove && IsValid(worldPosition))
        {
            _pendingMove = worldPosition;
            FreezePreview();
            _confirmMove = true;
            _uIManagerBattle.ShowConfirmMove();
        }
    }

    public void OnMovePerformed()
    {
        var newUnit = _battleBoard.Build(_pendingMove, _activeUnit.UnitData.UnitPrefab, _activeUnit.UnitData);
        _battleBoard.RemoveUnit(_activeUnit);
        _activeUnit = newUnit;
        HidePlayerMove();
        StopPreview();
        _confirmMove = false;
        _uIManagerBattle.UnitController.SetAlreadyMove(true);
        UIManagerBattle.ShowUnitAction(newUnit, _pendingMove);
    }

    public void OnMoveCanceled()
    {
        HidePlayerMove();
        StopPreview();
        _confirmMove = false;
    }
    public void OnAttackButton()
    {
        if (ActiveUnit == null) return;
        PerformAttack();
        HidePlayerAttack();
        UIManagerBattle.HideUnitAction();
        SetActiveUnit(null);
        CheckEnemies();
    }
    public void PerformAttack()
    {
        var attacker = ActiveUnit;
        var origin = attacker.Coordinates;
        var data = attacker.UnitData;
        var offsets = UnitAttackCalculate.GetOffsets(
            data.AttackPattern, data.Range, data.Direction
        );

        foreach (var off in offsets)
        {
            var cell = origin + off;
            Vector3 world = _battleBoard.CellToWorld(cell) + new Vector3(0.5f, 0.5f);
            var target = _battleBoard.GetUnit(world);
            UIManagerBattle.StartVFXExplosive(world);
            if (target == null ) continue;
            Debug.Log(target.IsItem);
            if (target.UnitData)
            {
                target.ChangeStatus(true); 
                
                target.UnitController.PlayDeadAnim();
                Debug.Log(target.UnitData.Name);
                if (target.UnitData.UnitSide == UnitSide.Player)
                {
                    var card = GetPlayerCard(target.UnitData);
                    UIManagerBattle.RemoveCard(card);
                }
            }
            if (target.IsItem)
            {
                TriggerItem(target);
            }
        }
        // attacker juga mati setelah menyerang
        attacker.UnitController.PlayAttackAnim();
        attacker.ChangeStatus(true);
        var cardAttacker = GetPlayerCard(attacker.UnitData);
        UIManagerBattle.RemoveCard(cardAttacker);
        _battleBoard.RemoveUnit(attacker);
    }

    public void TriggerItem(UnitModel itemModel)
    {
        var origin   = itemModel.Coordinates;
        var data     = itemModel.ItemData;
        var offsets  = UnitAttackCalculate.GetOffsets(
            data.AttackPattern, data.AttacKRange, data.AttackDirection
        );
        foreach (var off in offsets) {
            var cell = origin + off;
            Vector3 world = _battleBoard.CellToWorld(cell) + new Vector3(0.5f, 0.5f);
            var target = _battleBoard.GetUnit(world);
            UIManagerBattle.StartVFXExplosive(world);
            if (target != null && target.UnitData)
            {
                target.ChangeStatus(true);
                target.UnitController.PlayDeadAnim();
                Debug.Log(target.UnitData.Name);
                if (target.UnitData.UnitSide == UnitSide.Player)
                {
                    var card = GetPlayerCard(target.UnitData);
                    UIManagerBattle.RemoveCard(card);
                }
            }
        }
        // attacker juga mati setelah menyerang
        itemModel.ChangeStatus(true);
        _battleBoard.RemoveUnit(itemModel);
    }
    public void OnAttackCanceled()
    {
        HidePlayerAttack();
    }

    public void OnStayPerformed()
    {
        SetActiveUnit(null);
        UIManagerBattle.UnitController.SetAlreadyMove(false);
        BattleState.ChangeState(EnemyTurnState);
    }
    public List<UnitModel> GetAliveUnitsBySide(UnitSide side)
    {
        return _battleBoard.GetUnits(side, true);
    }

    public void MoveEnemy(UnitModel enemy, Vector3 worldPosition)
    {
        var newUnit = _battleBoard.Build(worldPosition, enemy.UnitData.UnitPrefab, enemy.UnitData);
        _battleBoard.RemoveUnit(enemy);
        _activeUnit = newUnit;
    }

    public void CheckEnemies()
    {
        var enemies = GetAliveUnitsBySide(UnitSide.Enemy);
        var players = GetAliveUnitsBySide(UnitSide.Player);
        if (players.Count == 0)
        {
            if (enemies.Count == 0)
            {
                SetBattleResult(BattleResult.PlayerWin);
                BattleState.ChangeState(GameEndState);
                return;
            }
            SetBattleResult(BattleResult.EnemyWin);
            BattleState.ChangeState(GameEndState);
            return;
        }
        if (enemies.Count > 0) BattleState.ChangeState(EnemyTurnState);
        else
        {
            SetBattleResult(BattleResult.PlayerWin);
            BattleState.ChangeState(GameEndState);
        }
    }
    public bool IsInAttackRange(UnitModel attacker, UnitModel target)
    {
        var data = attacker.UnitData;
        var offsets = UnitAttackCalculate.GetOffsets(
            data.AttackPattern, data.Range, data.Direction
        );
        foreach (var off in offsets)
        {
            if (attacker.Coordinates + off == target.Coordinates)
                return true;
        }
        return false;
    }

    public Vector3 CellToWorld(Vector3Int baseCoord)
    {
        return _battleBoard.CellToWorld(baseCoord);
    }
    public UnitModel GetUnit(Vector3 worldPosition)
    {
        return _battleBoard.GetUnit(worldPosition);
    }
    public List<Vector3> GetAvailableMoveWorldPositions(UnitModel unit)
    {
        _battleArea.ShowMoveTile(unit);
        var worldPositions = _battleArea.GetValidMoveWorldPositions();
        _battleArea.HideMoveTile();
        return worldPositions;
    }
    public Vector3Int WorldToCell(Vector3 world)
        => _battleBoard.WorldToCell(world);

    public void GoToMainMenu()
    {
        SceneController.Instance.ChangeScene("MainMenu");
    }
}
