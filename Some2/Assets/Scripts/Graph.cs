using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Graph : MonoBehaviour {
    private LineRenderer m_line_renderer;
    private EdgeCollider2D m_edge_collider;

    private ASTNode m_previous_working_ast;
    private ASTNode m_target_function;
    private bool m_start_tinterp;
    private Evaluator m_evaluator;

    private float t = 0.0f;

    [Range(0.001f, 1.0f)]
    public float Resolution;
    public string SourceCode;

    private float f(float x) {
        return Mathf.Lerp(
            m_evaluator.Evaluate(m_previous_working_ast, x),
            m_evaluator.Evaluate(m_target_function, x),
            t);
    }

    void Start() {
        if (!m_line_renderer) m_line_renderer = GetComponent<LineRenderer>();
        if (!m_edge_collider) m_edge_collider = GetComponent<EdgeCollider2D>();

        m_evaluator = new Evaluator(SourceCode);
        m_previous_working_ast = m_evaluator.Parse();

        RecalculateGraph();
    }

    private bool IsInCameraBounds(Vector2 v) {
        const float tolerance = 1.0f;
        
        float half_height = Camera.main.orthographicSize + tolerance;
        float half_width = Camera.main.orthographicSize * Camera.main.aspect + tolerance;
        return v.x >= -half_width && v.x <= half_width && v.y >= -half_height && v.y <= half_height;
    }

    private void RecalculateGraph() {
        List<Vector2> points2D = new List<Vector2>();
        List<Vector3> points3D = new List<Vector3>();

        float half_width = Camera.main.orthographicSize * Camera.main.aspect;

        Vector2 last;
        Vector2 curr = new Vector2(-half_width - Resolution, f(-half_width - Resolution));
        Vector2 next = new Vector2(-half_width - Resolution, f(-half_width - Resolution));
        for (float x = -half_width - Resolution; x <= half_width + Resolution; x += Resolution) {
            last = curr;
            curr = next;
            next = new Vector2(x, f(x));

            if (IsInCameraBounds(curr)) {
                points2D.Add(curr);
                points3D.Add(curr);
            } else {
                if (IsInCameraBounds(last) || IsInCameraBounds(next)) {
                    points2D.Add(curr);
                    points3D.Add(curr);
                }
            }
        }

        Vector3[] v3d = points3D.ToArray();

        m_line_renderer.positionCount = v3d.Length;
        m_line_renderer.SetPositions(v3d);
        m_edge_collider.SetPoints(points2D);
    }

    void Update() {
        if (m_target_function != m_previous_working_ast) {
            if (t >= 1) {
                StopCoroutine("LerpT");
                t = 0.0f;
                m_previous_working_ast = m_target_function;
            }
            RecalculateGraph();
        }
    }

    public IEnumerator LerpT() {
        // Configurable resolution here too?
        float step = 0.004f;
        for (float x = 0; x <= 1 + step; x += step) {
            // Configurable Easing function here?
            // Cubic Ease In-Out from easings.net
            t = x < 0.5 ? 4 * x*x*x : 1 - ((-2 * x + 2)*(-2 * x + 2)*(-2 * x + 2)) / 2;
            yield return null;
        }
    }

    public void SetFuction(string code) {
        StartCoroutine("LerpT");
        t = 0.0f;
        SourceCode = code;
        m_evaluator.Reset(SourceCode);
        ASTNode node = m_evaluator.Parse();
        if (m_evaluator.errored)
            return; //node = new NumberASTNode(new Token(TokenType.Number, "0", 0.0f));
        m_target_function = node;
        RecalculateGraph();
    }
}
