using System.Collections.Generic;
using AxGrid;
using AxGrid.Base;
using UnityEngine;
using UnityEngine.UI;

namespace Task3.Slot
{
    public class ReelView : MonoBehaviourExt
    {
        [Header("Reel Items (UI Images in vertical line)")]
        [SerializeField] private List<RectTransform> itemRects = new List<RectTransform>();
        [SerializeField] private List<Image> itemImages = new List<Image>();

        [Header("Symbols (sprites)")]
        [SerializeField] private List<Sprite> sharpSprites = new List<Sprite>();
        [SerializeField] private List<Sprite> blurredSprites = new List<Sprite>();
        private int[] itemSymbolIndex;

        [Header("Blur by speed")]
        [SerializeField] private float blurOnSpeed = 800f;  
        [SerializeField] private float blurOffSpeed = 500f;
        private bool isBlurredNow = false;

        [Header("Layout")]
        [SerializeField] private float itemHeight = 300f;  
        [SerializeField] private int visibleCenterIndex = 2; 

        [Header("Recycle thresholds")]
        [SerializeField] private float recyclePadding = 0.5f;

        private System.Random rng = new System.Random();

        [OnStart]
        private void StartThis()
        {
            if (itemSymbolIndex == null || itemSymbolIndex.Length != itemImages.Count)
                itemSymbolIndex = new int[itemImages.Count];
        }

        public void ResetLayoutToGrid()
        {
            for (int i = 0; i < itemRects.Count; i++)
            {
                float y = ((itemRects.Count - 1) * 0.5f - i) * itemHeight;
                SetItemY(itemRects[i], y);
            }
        }

        public void RandomizeAllSymbols(bool blurred = false)
        {
            if (itemSymbolIndex == null || itemSymbolIndex.Length != itemImages.Count)
                itemSymbolIndex = new int[itemImages.Count];

            for (int i = 0; i < itemImages.Count; i++)
                SetRandomSymbol(i, blurred);
        }

        public void SetSpeed(float speed)
        {
            Settings.Model.Set("Speed", speed);
        }

        public float GetSpeed()
        {
            return Settings.Model.GetFloat("Speed", 0f);
        }

        [OnUpdate]
        private void MoveLoop()
        {
            float speed = Settings.Model.GetFloat("Speed", 0f);
            if (Mathf.Abs(speed) < 0.0001f) return;

            float absSpeed = Mathf.Abs(speed);
            float dy = speed * Time.deltaTime;

            if (!isBlurredNow && absSpeed >= blurOnSpeed)
            {
                isBlurredNow = true;
                SetAllBlurred(true);
            }
            else if (isBlurredNow && absSpeed <= blurOffSpeed)
            {
                isBlurredNow = false;
                SetAllBlurred(false);
            }

            if (absSpeed < 0.0001f) 
                return;

            for (int i = 0; i < itemRects.Count; i++)
            {
                float newY = itemRects[i].anchoredPosition.y - dy;
                SetItemY(itemRects[i], newY);
            }

            RecycleIfNeeded();
        }

        private void RecycleIfNeeded()
        {
            float topMost = float.NegativeInfinity;
            float bottomMost = float.PositiveInfinity;

            for (int i = 0; i < itemRects.Count; i++)
            {
                float y = itemRects[i].anchoredPosition.y;
                if (y > topMost) topMost = y;
                if (y < bottomMost) bottomMost = y;
            }

            float outThreshold = bottomMost - itemHeight * recyclePadding;
            float bottomLimit = -(itemHeight * (itemRects.Count - 1) * 0.5f) - itemHeight * recyclePadding;

            for (int i = 0; i < itemRects.Count; i++)
            {
                float y = itemRects[i].anchoredPosition.y;
                if (y < bottomLimit)
                {
                    float newY = topMost + itemHeight;
                    SetItemY(itemRects[i], newY);
                    topMost = newY;
                    SetRandomSymbol(i, blurred: isBlurredNow);
                }
            }
        }

        private void SetRandomSymbol(int itemId, bool blurred)
        {
            if (sharpSprites == null || blurredSprites == null) 
                return;
            if (sharpSprites.Count == 0 || blurredSprites.Count == 0) 
                return;

            int count = Mathf.Min(sharpSprites.Count, blurredSprites.Count);
            int idx = rng.Next(0, count);

            itemSymbolIndex[itemId] = idx;
            itemImages[itemId].sprite = blurred ? blurredSprites[idx] : sharpSprites[idx];
        }

        private void SetItemY(RectTransform rt, float y)
        {
            var p = rt.anchoredPosition;
            p.y = y;
            rt.anchoredPosition = p;
        }

        public int SnapToCenterAndGetResultIndex(float snapTimeSeconds)
        {
            int best = -1;
            float bestAbs = float.PositiveInfinity;

            for (int i = 0; i < itemRects.Count; i++)
            {
                float abs = Mathf.Abs(itemRects[i].anchoredPosition.y);
                if (abs < bestAbs)
                {
                    bestAbs = abs;
                    best = i;
                }
            }

            if (best < 0) return -1;

            float delta = -itemRects[best].anchoredPosition.y;

            int resultIndex = SpriteToIndex(itemImages[best].sprite);

            CacheSnap(delta, snapTimeSeconds);

            return resultIndex;
        }

        private float snapDelta;
        private List<float> snapStartY;

        private void CacheSnap(float delta, float time)
        {
            snapDelta = delta;
            snapStartY = new List<float>(itemRects.Count);
            for (int i = 0; i < itemRects.Count; i++)
                snapStartY.Add(itemRects[i].anchoredPosition.y);
        }

        public void ApplySnapProgress(float t01)
        {
            if (snapStartY == null || snapStartY.Count != itemRects.Count) 
                return;
            t01 = Mathf.Clamp01(t01);

            for (int i = 0; i < itemRects.Count; i++)
            {
                float y = Mathf.Lerp(snapStartY[i], snapStartY[i] + snapDelta, t01);
                SetItemY(itemRects[i], y);
            }
        }

        private int SpriteToIndex(Sprite s)
        {
            if (s == null || sharpSprites == null) return -1;
            return sharpSprites.IndexOf(s);
        }

        public void SetAllBlurred(bool blurred)
        {
            int count = Mathf.Min(sharpSprites.Count, blurredSprites.Count);
            for (int i = 0; i < itemImages.Count; i++)
            {
                int idx = itemSymbolIndex[i];
                idx = Mathf.Clamp(idx, 0, count - 1);
                itemImages[i].sprite = blurred ? blurredSprites[idx] : sharpSprites[idx];
            }
        }
    }
}