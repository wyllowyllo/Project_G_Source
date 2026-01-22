using System;
using System.Text;
using Combat.Core;
using Equipment;
using Progression;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace Debugging
{
    // 플레이어 종합 디버그 UI입니다.
    // 상태 확인 및 테스트용 조작 기능을 제공합니다.
    public class PlayerDebugUI : MonoBehaviour
    {
        [Header("References")]
        [SerializeField] private PlayerProgression _progression;
        [SerializeField] private Combatant _combatant;

        [Header("Debug Settings")]
        [SerializeField] private int _debugXp = 100;
        [SerializeField] private float _debugDamage = 10f;
        [SerializeField] private float _debugHeal = 20f;
        [SerializeField] private KeyCode _toggleKey = KeyCode.F1;

        private GameObject _panel;
        private Text _infoText;
        private RectTransform _xpFill;
        private RectTransform _hpFill;
        private RectTransform _content;

        private void Start()
        {
            CreateUI();
        }

        private void Update()
        {
            if (Input.GetKeyDown(_toggleKey))
                _panel.SetActive(!_panel.activeSelf);

            if (_panel.activeSelf)
                RefreshUI();
        }

        private void CreateUI()
        {
            CreateEventSystem();
            var canvas = CreateCanvas();
            _panel = CreatePanel(canvas.transform);

            var scrollView = CreateScrollView(_panel.transform);
            _content = scrollView.GetComponent<ScrollRect>().content;

            CreateText(_content, "Player Debug (F1)", 20, FontStyle.Bold, 28);
            CreateSpacer(_content, 10);

            CreateText(_content, "--- Progression ---", 14, FontStyle.Bold, 20);
            _infoText = CreateText(_content, "", 12, FontStyle.Normal, 120);
            _xpFill = CreateBar(_content, new Color(0.2f, 0.6f, 1f));

            CreateButtonRow(_content, new[] {
                ($"+{_debugXp} XP", () => _progression?.AddExperience(_debugXp)),
                ("+1000 XP", (Action)(() => _progression?.AddExperience(1000)))
            });
            CreateButtonRow(_content, new[] {
                ("Lv+1", () => _progression?.SetLevel(_progression.Level + 1)),
                ("Lv-1", () => _progression?.SetLevel(_progression.Level - 1)),
                ("Max", (Action)(() => _progression?.SetLevel(30)))
            });

            CreateSpacer(_content, 10);
            CreateText(_content, "--- Health ---", 14, FontStyle.Bold, 20);
            _hpFill = CreateBar(_content, new Color(0.2f, 0.8f, 0.2f));
            CreateButtonRow(_content, new[] {
                ("Damage", () => _combatant?.TakeDamage(_debugDamage)),
                ("Heal", () => _combatant?.Heal(_debugHeal)),
                ("Full", (Action)(() => _combatant?.Heal(_combatant?.MaxHealth ?? 0)))
            });

            CreateSpacer(_content, 10);
            CreateButton(_content, "Unequip All", () => {
                var dataManager = EquipmentDataManager.Instance;
                if (dataManager == null) return;
                foreach (EquipmentSlot slot in Enum.GetValues(typeof(EquipmentSlot)))
                    dataManager.Unequip(slot);
            });
        }

        private void RefreshUI()
        {
            var sb = new StringBuilder();

            if (_progression != null)
            {
                sb.AppendLine($"Level: {_progression.Level} ({_progression.Rank})");
                sb.AppendLine($"XP: {_progression.CurrentXp} / {_progression.XpToNextLevel}");
                _xpFill.anchorMax = new Vector2(_progression.LevelProgress, 1f);
            }

            if (_combatant != null)
            {
                sb.AppendLine($"HP: {_combatant.CurrentHealth:F0} / {_combatant.MaxHealth:F0}");
                _hpFill.anchorMax = new Vector2(_combatant.MaxHealth > 0 ? _combatant.CurrentHealth / _combatant.MaxHealth : 0f, 1f);

                var stats = _combatant.Stats;
                sb.AppendLine($"ATK: {stats.AttackDamage.Value:F0}  DEF: {stats.Defense.Value:F0}");
                sb.AppendLine($"Crit: {stats.CriticalChance.Value:P0} / {stats.CriticalMultiplier.Value:P0}");
            }

            var equipmentDataManager = EquipmentDataManager.Instance;
            if (equipmentDataManager != null)
            {
                foreach (EquipmentSlot slot in Enum.GetValues(typeof(EquipmentSlot)))
                {
                    var item = equipmentDataManager.GetEquipment(slot);
                    if (item != null)
                        sb.AppendLine($"{slot}: {item.EquipmentName} ({item.Grade})");
                }
            }

            _infoText.text = sb.ToString();
        }

        private void CreateEventSystem()
        {
            if (FindAnyObjectByType<EventSystem>() != null) return;
            var obj = new GameObject("EventSystem");
            obj.AddComponent<EventSystem>();
            obj.AddComponent<StandaloneInputModule>();
        }

        private Canvas CreateCanvas()
        {
            var obj = new GameObject("DebugCanvas");
            obj.transform.SetParent(transform);
            var canvas = obj.AddComponent<Canvas>();
            canvas.renderMode = RenderMode.ScreenSpaceOverlay;
            canvas.sortingOrder = 100;
            obj.AddComponent<CanvasScaler>();
            obj.AddComponent<GraphicRaycaster>();
            return canvas;
        }

        private GameObject CreatePanel(Transform parent)
        {
            var obj = new GameObject("Panel");
            obj.transform.SetParent(parent, false);

            var image = obj.AddComponent<Image>();
            image.color = new Color(0.1f, 0.1f, 0.1f, 0.9f);

            var rect = obj.GetComponent<RectTransform>();
            rect.anchorMin = new Vector2(0, 1);
            rect.anchorMax = new Vector2(0, 1);
            rect.pivot = new Vector2(0, 1);
            rect.anchoredPosition = new Vector2(10, -10);
            rect.sizeDelta = new Vector2(280, 400);

            return obj;
        }

        private GameObject CreateScrollView(Transform parent)
        {
            var scrollObj = new GameObject("ScrollView");
            scrollObj.transform.SetParent(parent, false);

            var scrollRect = scrollObj.AddComponent<ScrollRect>();
            scrollRect.horizontal = false;
            scrollRect.vertical = true;
            scrollRect.movementType = ScrollRect.MovementType.Clamped;

            var scrollRectTransform = scrollObj.GetComponent<RectTransform>();
            scrollRectTransform.anchorMin = Vector2.zero;
            scrollRectTransform.anchorMax = Vector2.one;
            scrollRectTransform.offsetMin = new Vector2(5, 5);
            scrollRectTransform.offsetMax = new Vector2(-5, -5);

            var viewport = new GameObject("Viewport");
            viewport.transform.SetParent(scrollObj.transform, false);
            var viewportImage = viewport.AddComponent<Image>();
            viewportImage.color = new Color(0, 0, 0, 0);
            viewport.AddComponent<RectMask2D>();

            var viewportRect = viewport.GetComponent<RectTransform>();
            viewportRect.anchorMin = Vector2.zero;
            viewportRect.anchorMax = Vector2.one;
            viewportRect.offsetMin = Vector2.zero;
            viewportRect.offsetMax = Vector2.zero;

            var content = new GameObject("Content");
            content.transform.SetParent(viewport.transform, false);
            var contentRect = content.AddComponent<RectTransform>();
            contentRect.anchorMin = new Vector2(0, 1);
            contentRect.anchorMax = new Vector2(1, 1);
            contentRect.pivot = new Vector2(0.5f, 1);
            contentRect.anchoredPosition = Vector2.zero;
            contentRect.sizeDelta = new Vector2(0, 1000);

            var layout = content.AddComponent<VerticalLayoutGroup>();
            layout.padding = new RectOffset(5, 5, 5, 5);
            layout.spacing = 3;
            layout.childControlWidth = true;
            layout.childControlHeight = true;
            layout.childForceExpandWidth = true;
            layout.childForceExpandHeight = false;
            layout.childAlignment = TextAnchor.UpperCenter;

            var fitter = content.AddComponent<ContentSizeFitter>();
            fitter.horizontalFit = ContentSizeFitter.FitMode.Unconstrained;
            fitter.verticalFit = ContentSizeFitter.FitMode.PreferredSize;

            scrollRect.viewport = viewportRect;
            scrollRect.content = contentRect;

            return scrollObj;
        }

        private Text CreateText(RectTransform parent, string text, int fontSize, FontStyle style, float height)
        {
            var obj = new GameObject("Text");
            obj.transform.SetParent(parent, false);

            var t = obj.AddComponent<Text>();
            t.text = text;
            t.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
            t.fontSize = fontSize;
            t.fontStyle = style;
            t.color = Color.white;
            t.alignment = TextAnchor.UpperLeft;

            var layout = obj.AddComponent<LayoutElement>();
            layout.minHeight = height;
            layout.preferredHeight = height;

            return t;
        }

        private RectTransform CreateBar(RectTransform parent, Color fillColor)
        {
            var bg = new GameObject("BarBg");
            bg.transform.SetParent(parent, false);
            var bgImage = bg.AddComponent<Image>();
            bgImage.color = new Color(0.2f, 0.2f, 0.2f);

            var layout = bg.AddComponent<LayoutElement>();
            layout.minHeight = 15;
            layout.preferredHeight = 15;

            var fill = new GameObject("Fill");
            fill.transform.SetParent(bg.transform, false);
            var fillImage = fill.AddComponent<Image>();
            fillImage.color = fillColor;

            var fillRect = fill.GetComponent<RectTransform>();
            fillRect.anchorMin = Vector2.zero;
            fillRect.anchorMax = new Vector2(0.5f, 1f);
            fillRect.sizeDelta = Vector2.zero;
            fillRect.anchoredPosition = Vector2.zero;

            return fillRect;
        }

        private void CreateButton(RectTransform parent, string text, Action onClick)
        {
            var obj = new GameObject("Button");
            obj.transform.SetParent(parent, false);

            var image = obj.AddComponent<Image>();
            image.color = new Color(0.3f, 0.3f, 0.3f);

            var button = obj.AddComponent<Button>();
            button.onClick.AddListener(() => onClick?.Invoke());

            var layout = obj.AddComponent<LayoutElement>();
            layout.minHeight = 28;
            layout.preferredHeight = 28;

            var textObj = new GameObject("Text");
            textObj.transform.SetParent(obj.transform, false);
            var t = textObj.AddComponent<Text>();
            t.text = text;
            t.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
            t.fontSize = 12;
            t.color = Color.white;
            t.alignment = TextAnchor.MiddleCenter;

            var textRect = textObj.GetComponent<RectTransform>();
            textRect.anchorMin = Vector2.zero;
            textRect.anchorMax = Vector2.one;
            textRect.offsetMin = Vector2.zero;
            textRect.offsetMax = Vector2.zero;
        }

        private void CreateButtonRow(RectTransform parent, (string text, Action onClick)[] buttons)
        {
            var row = new GameObject("ButtonRow");
            row.transform.SetParent(parent, false);

            var rowLayout = row.AddComponent<HorizontalLayoutGroup>();
            rowLayout.spacing = 5;
            rowLayout.childControlWidth = true;
            rowLayout.childControlHeight = true;
            rowLayout.childForceExpandWidth = true;
            rowLayout.childForceExpandHeight = true;

            var rowLayoutElement = row.AddComponent<LayoutElement>();
            rowLayoutElement.minHeight = 28;
            rowLayoutElement.preferredHeight = 28;

            foreach (var (text, onClick) in buttons)
            {
                var obj = new GameObject("Button");
                obj.transform.SetParent(row.transform, false);

                var image = obj.AddComponent<Image>();
                image.color = new Color(0.3f, 0.3f, 0.3f);

                var button = obj.AddComponent<Button>();
                button.onClick.AddListener(() => onClick?.Invoke());

                var textObj = new GameObject("Text");
                textObj.transform.SetParent(obj.transform, false);
                var t = textObj.AddComponent<Text>();
                t.text = text;
                t.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
                t.fontSize = 11;
                t.color = Color.white;
                t.alignment = TextAnchor.MiddleCenter;

                var textRect = textObj.GetComponent<RectTransform>();
                textRect.anchorMin = Vector2.zero;
                textRect.anchorMax = Vector2.one;
                textRect.offsetMin = Vector2.zero;
                textRect.offsetMax = Vector2.zero;
            }
        }

        private void CreateSpacer(RectTransform parent, float height)
        {
            var obj = new GameObject("Spacer");
            obj.transform.SetParent(parent, false);
            var layout = obj.AddComponent<LayoutElement>();
            layout.minHeight = height;
            layout.preferredHeight = height;
        }
    }
}
