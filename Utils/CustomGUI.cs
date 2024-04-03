using static UnityEngine.UI.CanvasScaler;
using UnityEngine.UI;
using UnityEngine;



namespace LethalMystery.Utils
{
    public class AssignmentUI : MonoBehaviour
    {
        private enum State
        {
            Showing,
            Shown,
            Hiding,
            Hidden
        }

        private readonly Color NONE_TEXT_COLOR = new Color(1f, 0.8277124f, 0.5235849f, 0.3254902f);

        private readonly Color BG_COLOR = new Color(1f, 0.6916346f, 0.259434f, 1f);

        private readonly Color TITLE_COLOR = new Color(1f, 0.9356132f, 0.8160377f, 1f);

        private readonly Color TEXT_COLOR = new Color(0.3584906f, 0.2703371f, 0f, 1f);

        private const float SHOW_SPEED = 1f;

        private const float HIDE_SPEED = 2f;

        private readonly Vector2 SHOW_POSITION = new Vector2(-50f, -350f);

        private readonly Vector2 HIDE_POSITION = new Vector2(500f, -350f);

        private Canvas? _canvas;

        private GameObject? _noneText;

        private RectTransform? _assignment;

        private static Text? _assignmentTitle;

        private static Text? _assignmentText;

        private Font? _font;

        private QuickMenuManager? _menuManager;

        private State _state;

        private float _animationProgress;

        //public Roles? currentRole;

        private void Awake()
        {
            UnityEngine.Object.DontDestroyOnLoad((UnityEngine.Object)(object)((Component)this).gameObject);
            _state = State.Hidden;
            ((Component)this).gameObject.layer = 5;
            _font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
            _canvas = new GameObject("Canvas").AddComponent<Canvas>();
            ((Component)_canvas).transform.SetParent(((Component)this).transform);
            _canvas.sortingOrder = -100;
            _canvas.renderMode = (RenderMode)0;
            RectTransform component = ((Component)_canvas).GetComponent<RectTransform>();
            component.sizeDelta = new Vector2(1920f, 1080f);
            component.anchorMin = Vector2.zero;
            component.anchorMax = Vector2.zero;
            component.pivot = Vector2.zero / 2f;
            CanvasScaler val = ((Component)_canvas).gameObject.AddComponent<CanvasScaler>();
            val.uiScaleMode = (CanvasScaler.ScaleMode)1;
            val.screenMatchMode = (ScreenMatchMode)2;
            val.referenceResolution = new Vector2(1920f, 1080f);
            CanvasGroup val3 = new GameObject("AssignmentPanel").AddComponent<CanvasGroup>();
            val3.alpha = 0.5f;
            Image val4 = ((Component)val3).gameObject.AddComponent<Image>();
            ((Graphic)val4).color = BG_COLOR;
            _assignment = ((Component)val3).gameObject.GetComponent<RectTransform>();
            ((Transform)_assignment).SetParent((Transform)(object)component);
            _assignment.pivot = Vector2.one;
            _assignment.anchorMin = Vector2.one;
            _assignment.anchorMax = Vector2.one;
            _assignment.anchoredPosition = new Vector2(-50f, -350f);
            _assignment.sizeDelta = new Vector2(350f, 80f);
            _assignmentTitle = new GameObject("Title").AddComponent<Text>();
            _assignmentTitle.font = _font;
            _assignmentTitle.fontSize = 20;
            _assignmentTitle.fontStyle = (FontStyle)1;
            _assignmentTitle.text = "No Current Role";
            ((Graphic)_assignmentTitle).color = TITLE_COLOR;
            _assignmentTitle.alignment = (TextAnchor)0;
            RectTransform component3 = ((Component)_assignmentTitle).gameObject.GetComponent<RectTransform>();
            ((Transform)component3).SetParent((Transform)(object)_assignment);
            component3.pivot = new Vector2(0.5f, 1f);
            component3.anchorMin = new Vector2(0f, 1f);
            component3.anchorMax = Vector2.one;
            component3.anchoredPosition = new Vector2(0f, -10f);
            component3.sizeDelta = new Vector2(-40f, 100f);
            _assignmentText = new GameObject("Text").AddComponent<Text>();
            _assignmentText.font = _font;
            _assignmentText.fontSize = 16;
            _assignmentText.fontStyle = (FontStyle)1;
            _assignmentText.text = "No Current Role";
            ((Graphic)_assignmentText).color = TEXT_COLOR;
            _assignmentText.alignment = (TextAnchor)6;
            RectTransform component4 = ((Component)_assignmentText).gameObject.GetComponent<RectTransform>();
            ((Transform)component4).SetParent((Transform)(object)_assignment);
            component4.pivot = new Vector2(0.5f, 0.5f);
            component4.anchorMin = new Vector2(0f, 0f);
            component4.anchorMax = Vector2.one;
            component4.anchoredPosition = new Vector2(0f, -10f);
            component4.sizeDelta = new Vector2(-40f, -40f);
        }



        private const float EASE_IN_OUT_MAGIC1 = 1.7f;

        private const float EASE_IN_OUT_MAGIC2 = 2.5500002f;

        public static float EaseInOutBack(float x)
        {
            return (x < 0.5f) ? (MathF.Pow(2f * x, 2f) * (7.1000004f * x - 2.5500002f) / 2f) : ((MathF.Pow(2f * x - 2f, 2f) * (3.5500002f * (x * 2f - 2f) + 2.5500002f) + 2f) / 2f);
        }

        public static int WeightedRandom(int[] weights)
        {
            int num = 0;
            for (int i = 0; i < weights.Length; i++)
            {
                num += weights[i];
            }
            int num2 = UnityEngine.Random.Range(0, num);
            int num3 = 0;
            for (int j = 0; j < weights.Length; j++)
            {
                num3 += weights[j];
                if (num3 >= num2)
                {
                    return j;
                }
            }
            return 0;
        }



        public static void SetAssignment(Dictionary<string, string> role)
        {
            if (_assignmentTitle is not null)
                _assignmentTitle.text = $"Role: {role["topText"]}";
            if (_assignmentText is not null)
                _assignmentText.text = role["bottomText"];

        }

        public void ClearAssignment(bool force = false)
        {
            if (force || _state == State.Shown || _state == State.Showing)
            {
                ChangeState(State.Hiding);
                //currentRole = null;
            }
        }

        private void ChangeState(State state)
        {
            _state = state;
            _animationProgress = 0f;
        }

        private void PanelAnimation()
        {
            if (_state == State.Shown || _state == State.Hidden)
            {
                return;
            }
            if (_animationProgress >= 1f)
            {
                _state++;
                return;
            }
            float num = 1f;
            if (_state == State.Hiding)
            {
                num = 2f;
            }
            _animationProgress += num * Time.deltaTime;
            float num2 = _animationProgress;
            if (_state == State.Hiding)
            {
                num2 = 1f - num2;
            }
            if (_assignment is not null)
                _assignment.anchoredPosition = Vector2.Lerp(HIDE_POSITION, SHOW_POSITION, EaseInOutBack(num2));
        }

        private void Update()
        {
            PanelAnimation();
        }
    }
}
