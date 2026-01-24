using System.Collections.Generic;
using UnityEngine;
using UnityEngine.XR;
public class ScoreCalculator
{
    public class Frame
    {
        public int FirstThrow = 0;
        public int SecondThrow = 0;
        public int FrameScore = 0;
        public bool IsStrike = false;
        public bool IsSpare = false;
        public bool IsComplete = false;
    }

    private List<Frame> _frames = new List<Frame>();
    private int _currentFrameIndex = 0;
    private int _currentThrowInFrame = 1; 

    public ScoreCalculator()
    {
        for (int i = 0; i < 10; i++)
        {
            _frames.Add(new Frame());
        }
    }

    public void StartNewGame()
    {
        _frames.Clear();
        for (int i = 0; i < 10; i++)
        {
            _frames.Add(new Frame());
        }
        _currentFrameIndex = 0;
        _currentThrowInFrame = 1; 
    }

    public void RecordThrow(int pinsKnockedDown, int throwInFrame)
    {
        Frame currentFrame = _frames[_currentFrameIndex];

        if (throwInFrame == 1)
        {
            currentFrame.FirstThrow = pinsKnockedDown;

            if (pinsKnockedDown == 10)
            {
                currentFrame.IsStrike = true;
                currentFrame.IsComplete = true;
                Debug.Log($"Фрейм {_currentFrameIndex + 1}, бросок 1: STRIKE!");
                _currentFrameIndex++;
                _currentThrowInFrame = 1; 
            }
            else
            {
                _currentThrowInFrame = 2; 
            }
        }
        else if (throwInFrame == 2)
        {
            currentFrame.SecondThrow = pinsKnockedDown;

            if (currentFrame.FirstThrow + pinsKnockedDown == 10)
            {
                currentFrame.IsSpare = true;
                Debug.Log($"Фрейм {_currentFrameIndex + 1}, бросок 2: SPARE! ({currentFrame.FirstThrow}+{pinsKnockedDown})");
            }

            currentFrame.IsComplete = true;
            _currentFrameIndex++;
            _currentThrowInFrame = 1; 
        }
    }

    public int GetFirstThrowScore(int frameIndex = -1)
    {
        if (frameIndex == -1)
            frameIndex = _currentFrameIndex;

        if (frameIndex < 0 || frameIndex >= _frames.Count)
            return 0;

        return _frames[frameIndex].FirstThrow;
    }

    public int GetPinsLeftAfterFirstThrow(int frameIndex = -1)
    {
        if (frameIndex == -1)
            frameIndex = _currentFrameIndex;

        return 10 - GetFirstThrowScore(frameIndex);
    }

    public int CalculateTotalScore()
    {
        int totalScore = 0;

        for (int i = 0; i < _frames.Count; i++)
        {
            Frame frame = _frames[i];

            int frameScore = frame.FirstThrow + frame.SecondThrow;
            if (frame.IsComplete) 
            {
                if (frame.IsStrike)
                {
                    int bonus = GetStrikeBonus(i);
                    frameScore += bonus;
                    Debug.Log($"Фрейм {i + 1}: STRIKE! Бонус={bonus}, Итого={frameScore}");
                }
                else if (frame.IsSpare)
                {
                    int bonus = GetSpareBonus(i);
                    frameScore += bonus;
                    Debug.Log($"Фрейм {i + 1}: SPARE! Бонус={bonus}, Итого={frameScore}");
                }
            }

            frame.FrameScore = frameScore;
            totalScore += frameScore;

          
            if (!frame.IsComplete && i == _currentFrameIndex)
            {
                break;
            }
        }

        Debug.Log($"=== Итоговый счет: {totalScore} ===");
        return totalScore;
    }

    private int GetStrikeBonus(int frameIndex)
    {
        int bonus = 0;

        int throwsNeeded = 2;
        int currentFrame = frameIndex + 1;

        while (throwsNeeded > 0 && currentFrame < _frames.Count)
        {
            Frame frame = _frames[currentFrame];


            if (frame.FirstThrow > 0)
            {
                bonus += frame.FirstThrow;
                throwsNeeded--;


                if (frame.IsStrike && throwsNeeded > 0)
                {

                    currentFrame++;
                    continue;
                }
            }


            if (throwsNeeded > 0 && frame.SecondThrow > 0)
            {
                bonus += frame.SecondThrow;
                throwsNeeded--;
            }

            currentFrame++;
        }

        return bonus;
    }

    private int GetSpareBonus(int frameIndex)
    {
        if (frameIndex + 1 < _frames.Count)
        {
            return _frames[frameIndex + 1].FirstThrow;
        }

        return 0;
    }

    public int GetCurrentFrameIndex()
    {
        return _currentFrameIndex;
    }


    public int GetCurrentThrowInFrame()
    {
        return _currentThrowInFrame;
    }

    public Frame GetCurrentFrame()
    {
        if (_currentFrameIndex >= _frames.Count)
            return null;

        return _frames[_currentFrameIndex];
    }

    public string GetDebugInfo()
    {
        return $"Фрейм {_currentFrameIndex + 1}, бросок {_currentThrowInFrame}";
    }
}