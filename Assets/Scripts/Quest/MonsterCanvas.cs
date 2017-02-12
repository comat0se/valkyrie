﻿using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class MonsterCanvas : MonoBehaviour
{

    public int offset = 0;
    public static float monsterSize = 4;
    public Dictionary<string, MonsterIcon> icons;

    // Call to update list of monsters
    public void UpdateList()
    {
        // Clean up everything marked as 'monsters'
        foreach (GameObject go in GameObject.FindGameObjectsWithTag("monsters"))
            Object.Destroy(go);

        // New list
        icons = new Dictionary<string, MonsterIcon>();

        Game game = Game.Get();
        int index = 0;
        foreach (Quest.Monster m in game.quest.monsters)
        {
            icons.Add(m.monsterData.name, new MonsterIcon(m, index++));
        }

        DrawUp();
        DrawDown();

        UpdateStatus();
    }

    public void DrawUp()
    {
        Game game = Game.Get();
        if (game.quest.monsters.Count < 6 && offset == 0)
        {
            return;
        }
        if (offset == 0)
        {
            TextButton up = new TextButton(new Vector2(UIScaler.GetRight(-4.25f), 1), new Vector2(4, 2), "/\\", delegate { noAction(); }, Color.gray);
            up.ApplyTag("monsters");
        }
        else
        {
            TextButton up = new TextButton(new Vector2(UIScaler.GetRight(-4.25f), 1), new Vector2(4, 2), "/\\", delegate { Move(-1); });
            up.ApplyTag("monsters");
        }
    }

    public void DrawDown()
    {
        Game game = Game.Get();
        if (game.quest.monsters.Count < 6)
        {
            return;
        }
        if (game.quest.monsters.Count - offset <  6)
        {
            TextButton down = new TextButton(new Vector2(UIScaler.GetRight(-4.25f), 27), new Vector2(4, 2), "\\/", delegate { noAction(); }, Color.gray);
            down.ApplyTag("monsters");
        }
        else
        {
            TextButton down = new TextButton(new Vector2(UIScaler.GetRight(-4.25f), 27), new Vector2(4, 2), "\\/", delegate { Move(); });
            down.ApplyTag("monsters");
        }
    }

    public static void Move(int index = 1)
    {
        Game game = Game.Get();
        game.monsterCanvas.offset += index;
        game.monsterCanvas.UpdateList();
    }

    public static void noAction()
    {
    }

    public void UpdateStatus()
    {
        Game game = Game.Get();
        foreach (Quest.Monster m in game.quest.monsters)
        {
            icons[m.monsterData.name].Draw(offset);
        }
    }

    public class MonsterIcon
    {
        Quest.Monster m;

        Game game;

        Sprite iconSprite;
        Sprite frameSprite;

        int index;

        public MonsterIcon(Quest.Monster monster, int i = 0)
        {
            m = monster;
            index = i;

            game = Game.Get();

            Texture2D newTex = ContentData.FileToTexture(m.monsterData.image);
            Texture2D frameTex = Resources.Load("sprites/borders/Frame_Monster_1x1") as Texture2D;
            iconSprite = Sprite.Create(newTex, new Rect(0, 0, newTex.width, newTex.height), Vector2.zero, 1);
            frameSprite = Sprite.Create(frameTex, new Rect(0, 0, frameTex.width, frameTex.height), Vector2.zero, 1);
        }

        public void Draw(int offset)
        {
            if (index < offset)
            {
                return;
            }
            if (index > offset + 4)
            {
                return;
            }

            GameObject mImg = new GameObject("monsterImg" + m.monsterData.name);
            mImg.tag = "monsters";
            GameObject mImgFrame = new GameObject("monsterFrame" + m.monsterData.name);
            mImgFrame.tag = "monsters";
            mImg.transform.parent = game.uICanvas.transform;
            mImgFrame.transform.parent = game.uICanvas.transform;

            RectTransform trans = mImg.AddComponent<RectTransform>();
            trans.SetInsetAndSizeFromParentEdge(RectTransform.Edge.Top, (3.75f + ((index - offset) * 4.5f)) * UIScaler.GetPixelsPerUnit(), monsterSize * UIScaler.GetPixelsPerUnit());
            trans.SetInsetAndSizeFromParentEdge(RectTransform.Edge.Right, 0.25f * UIScaler.GetPixelsPerUnit(), monsterSize * UIScaler.GetPixelsPerUnit());
            mImg.AddComponent<CanvasRenderer>();
            RectTransform transFrame = mImgFrame.AddComponent<RectTransform>();
            transFrame.SetInsetAndSizeFromParentEdge(RectTransform.Edge.Top, (3.75f + ((index - offset) * 4.5f)) * UIScaler.GetPixelsPerUnit(), monsterSize * UIScaler.GetPixelsPerUnit());
            transFrame.SetInsetAndSizeFromParentEdge(RectTransform.Edge.Right, 0.25f * UIScaler.GetPixelsPerUnit(), monsterSize * UIScaler.GetPixelsPerUnit());
            mImgFrame.AddComponent<CanvasRenderer>();

            UnityEngine.UI.Image icon = mImg.AddComponent<UnityEngine.UI.Image>();
            icon.sprite = iconSprite;
            icon.rectTransform.sizeDelta = new Vector2(monsterSize * UIScaler.GetPixelsPerUnit() * 0.83f, monsterSize * UIScaler.GetPixelsPerUnit() * 0.83f);

            UnityEngine.UI.Image iconFrame = mImgFrame.AddComponent<UnityEngine.UI.Image>();
            iconFrame.sprite = frameSprite;
            iconFrame.rectTransform.sizeDelta = new Vector2(monsterSize * UIScaler.GetPixelsPerUnit(), monsterSize * UIScaler.GetPixelsPerUnit());

            UnityEngine.UI.Button buttonFrame = mImgFrame.AddComponent<UnityEngine.UI.Button>();
            buttonFrame.interactable = true;
            buttonFrame.onClick.AddListener(delegate { MonsterDiag(m.monsterData.name); });

            if (m.activated && m.unique)
            {
                iconFrame.color = new Color(0f, 0.3f, 0f, 1);
                icon.color = new Color(0.5f, 0.5f, 0.5f, 1);
            }
            else if (m.activated)
            {
                iconFrame.color = new Color((float)0.2, (float)0.2, (float)0.2, 1);
                icon.color = new Color(0.5f, 0.5f, 0.5f, 1);
            }
            else if (m.unique)
            {
                iconFrame.color = new Color(0.6f, 1f, 0.6f, 1);
            }
            else
            {
                iconFrame.color = Color.white;
                icon.color = Color.white;
            }

        }

        static void MonsterDiag(string name)
        {
            // If there are any other dialogs open just finish
            if (GameObject.FindGameObjectWithTag("dialog") != null)
                return;

            Game game = Game.Get();
            foreach (Quest.Monster m in game.quest.monsters)
            {
                if (name.Equals(m.monsterData.name))
                {
                    if (game.gameType.DisplayHeroes())
                    {
                        new MonsterDialog(m);
                    }
                    else
                    {
                        new MonsterDialogMoM(m);
                    }
                }
            }
        }
    }
}
