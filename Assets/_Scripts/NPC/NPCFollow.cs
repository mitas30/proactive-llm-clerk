using UnityEngine;
using UnityEngine.AI;

[RequireComponent(typeof(NavMeshAgent), typeof(Animator))]
public class NPCFollow : MonoBehaviour
{
    [SerializeField] private Transform player;
    [SerializeField] private float stopDistance = 1.2f;   // 望む停止距離
    [SerializeField] private float repathInterval = 0.1f; // 目標更新間隔
    [SerializeField] private float turnSpeed = 8f;        // 手動回転の速さ（agent.updateRotation=false のとき有効）
    [SerializeField] private bool rotateManually = true;  // 目を見るために手動回転するか
    [SerializeField] private bool isFollowing = false;
    private NavMeshAgent agent;
    private Animator anim;
    private float repathTimer;
    private bool requestImmediatePath; // フォロー開始直後に即座に目的地をセットするリクエスト

    static readonly int WalkHash = Animator.StringToHash("Walk");

    public bool IsFollowing { get { return isFollowing; } set { isFollowing = value; } }

    void Awake()
    {
        agent = GetComponent<NavMeshAgent>();
        anim = GetComponent<Animator>();

        agent.stoppingDistance = stopDistance;        // NavMeshAgent側にも同期
        agent.autoBraking = true;

        if (rotateManually)
            agent.updateRotation = false;             // 回転は自前で行う
    }

    void Update()
    {
        if (player == null) return;

        // IsFollowing=false の間は「向くだけ」: 経路更新・移動・Walkアニメを止める
        if (!isFollowing)
        {
            // NavMeshAgent を完全停止し、残留経路をクリア
            if (!agent.isStopped) agent.isStopped = true;
            if (agent.hasPath) agent.ResetPath();

            // アニメーションは歩かせない
            anim.SetBool(WalkHash, false);

            // 手動回転で常にプレイヤー方向を向く（水平回転）
            if (rotateManually)
            {
                Vector3 targetXZ_onlyY = new Vector3(player.position.x, transform.position.y, player.position.z);
                Vector3 dirOnlyY = targetXZ_onlyY - transform.position;
                if (dirOnlyY.sqrMagnitude > 0.0001f)
                {
                    Quaternion targetRot = Quaternion.LookRotation(dirOnlyY.normalized, Vector3.up);
                    transform.rotation = Quaternion.Slerp(transform.rotation, targetRot, Time.deltaTime * turnSpeed);
                }
            }

            return;
        }

        // 目標点を「カメラの真下の地面」に射影
        Vector3 targetXZ = new Vector3(player.position.x, transform.position.y, player.position.z);

        // 可能ならナビメッシュ上の最近傍に吸着
        Vector3 navTarget = targetXZ;
        if (NavMesh.SamplePosition(targetXZ, out NavMeshHit hit, 2.0f, agent.areaMask))
            navTarget = hit.position;

        // 一定間隔でだけ経路更新
        repathTimer -= Time.deltaTime;
        if (repathTimer <= 0f || requestImmediatePath)
        {
            agent.SetDestination(navTarget);
            repathTimer = repathInterval;
            requestImmediatePath = false;
        }

        // 停止条件は remainingDistance を優先
        bool hasPath = agent.hasPath && !agent.pathPending;
        bool shouldMove = !hasPath || agent.remainingDistance > agent.stoppingDistance + 0.05f;

        agent.isStopped = !shouldMove;
        anim.SetBool(WalkHash, shouldMove);

        // 水平方向に回転させる
        if (rotateManually)
        {
            Vector3 dir = targetXZ - transform.position;
            if (dir.sqrMagnitude > 0.0001f)
            {
                Quaternion targetRot = Quaternion.LookRotation(dir.normalized, Vector3.up);
                transform.rotation = Quaternion.Slerp(transform.rotation, targetRot, Time.deltaTime * turnSpeed);
            }
        }
    }

    public void OnFinishTutorial()
    {
        isFollowing = true;
        // 次のUpdateで即座に目的地セットを走らせ、再開ラグを抑える
        requestImmediatePath = true;
    }
}
