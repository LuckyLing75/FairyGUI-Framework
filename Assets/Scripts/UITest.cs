using FGUIFW;

public class UITest : FGUIBase
{
    public override UIGroup UIGroup => UIGroup.主界面;

    public override UIHierarchyType UIHierarchyType => UIHierarchyType.固定层;

    public UITestComs uiTestComs;

    public override void OnInstantiate()
    {
        base.OnInstantiate();

        uiTestComs = InitFGUIComs<UITestComs>();
    }
}