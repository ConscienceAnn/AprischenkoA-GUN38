using System;


namespace Game.Casino
{
    public abstract class CasinoGameBas
    {
        
        public event Action<int> OnWin;  
        public event Action<int> OnLose;  
        public event Action OnDraw;       

        protected void OnWinInvoke(int winAmount) => OnWin?.Invoke(winAmount);
        protected void OnLoseInvoke(int loseAmount) => OnLose?.Invoke(loseAmount);
        protected void OnDrawInvoke() => OnDraw?.Invoke();

        public abstract void PlayGame();

        protected abstract void FactoryMethod();

        protected CasinoGameBas()
        {
            FactoryMethod();
        }
    }
}
