using System;

namespace Heck.PlayView;

public interface IPlayViewController
{
    public event Action? Finished;

    public bool Init(StartStandardLevelParameters standardLevelParameters);
}
