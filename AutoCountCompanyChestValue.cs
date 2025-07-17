using System;
using DailyRoutines.Abstracts;
using Dalamud.Game.Addon.Lifecycle;
using Dalamud.Game.Addon.Lifecycle.AddonArgTypes;
using OmenTools;
using FFXIVClientStructs.FFXIV.Client.Game;
using FFXIVClientStructs.FFXIV.Component.GUI;
using ImGuiNET;

namespace DailyRoutines.ModulesPublic;

public class AutoCountCompanyChestValue : DailyModuleBase
{
    public override ModuleInfo Info { get; } = new()
    {
        Title = GetLoc("统计部队箱中的沉船首饰"),
        Description = GetLoc("计算部队箱中的沉船首饰的数量和价值"),
        Category = ModuleCategories.UIOptimization,
        Author = ["采购"]
    };
    private bool showWindow = false; // 控制窗口显示
    private IntPtr fcChestPtr = IntPtr.Zero; // 缓存部队箱界面指针

    public override void Init()
    {
        TaskHelper ??= new() { TimeLimitMS = 5_000 };
        DService.AddonLifecycle.RegisterListener(AddonEvent.PreFinalize, "FreeCompanyChest", OnFCChestAddon);
        DService.UiBuilder.Draw += DrawUI; // 注册ImGui绘制
    }
    public override void Uninit()
    {
        DService.AddonLifecycle.UnregisterListener(OnFCChestAddon);
        DService.UiBuilder.Draw -= DrawUI; // 注销ImGui绘制
        base.Uninit();
    }

    private void OnFCChestAddon(AddonEvent type, AddonArgs? args)
    {
        TaskHelper.Abort();
        showWindow = false;
        fcChestPtr = IntPtr.Zero;
    }
    //画UI
    private void DrawUI()
    {
        // 查找部队箱界面
        var ui = DService.Gui.GetAddonByName("FreeCompanyChest", 1);
        showWindow = ui != IntPtr.Zero;
        fcChestPtr = ui;

        if (!showWindow) return;

        ImGui.SetNextWindowSize(new System.Numerics.Vector2(300, 100), ImGuiCond.Always);
        ImGui.SetNextWindowPos(new System.Numerics.Vector2(200, 200), ImGuiCond.FirstUseEver);

        //imgui界面并调用Check方法
        if (ImGui.Begin("部队箱价值统计", ImGuiWindowFlags.NoResize | ImGuiWindowFlags.NoCollapse|ImGuiWindowFlags.NoTitleBar))
        {
            ImGui.Text("统计当前页面所有沉船首饰价值");
            if (ImGui.Button("点我输出在聊天框"))
            {
                unsafe
                {
                    Check((FFXIVClientStructs.FFXIV.Component.GUI.AtkUnitBase*)fcChestPtr);
                }
            }
        }
        ImGui.End();
    }
    //计算当前界面的首饰价值并输出
    private unsafe void Check(AtkUnitBase* addon)
    {
        var fcPage   = GetCurrentFCPage(addon);
        var manager = InventoryManager.Instance();
        if (manager == null) return;
        var id22500 = manager->GetItemCountInContainer(22500, fcPage, false);
        var id22501 = manager->GetItemCountInContainer(22501, fcPage, false);
        var id22502 = manager->GetItemCountInContainer(22502, fcPage, false);
        var id22503 = manager->GetItemCountInContainer(22503, fcPage, false);
        var id22504 = manager->GetItemCountInContainer(22504, fcPage, false);
        var id22505 = manager->GetItemCountInContainer(22505, fcPage, false);
        var id22506 = manager->GetItemCountInContainer(22506, fcPage, false);
        var id22507 = manager->GetItemCountInContainer(22507, fcPage, false);
        var total =id22500*8000 + id22501*9000 + id22502*10000 + id22503*13000 + id22504*27000 + id22505*28500 + id22506*30000 + id22507*34500;
        DService.Chat.Print("上等沉船戒指(2w7)数量:"+id22504);
        DService.Chat.Print("上等沉船手镯(2w85)数量:"+id22505);
        DService.Chat.Print("上等沉船耳饰(3w)数量:"+id22506);
        DService.Chat.Print("上等沉船项链(3w45)数量:"+id22507);
        DService.Chat.Print("沉船戒指(8k)数量:"+id22500);
        DService.Chat.Print("沉船手镯(9k)数量:"+id22501);
        DService.Chat.Print("沉船手镯(1w)数量:"+id22502);
        DService.Chat.Print("沉船手镯(1w3)数量:"+id22503);
        DService.Chat.Print("当前页面的所有首饰价值为:"+total);
    }
    //获取当前部队箱的InventoryType
    private static unsafe InventoryType GetCurrentFCPage(AtkUnitBase* addon) => 
        addon == null ? InventoryType.FreeCompanyPage1 : (InventoryType)(20000 + addon->AtkValues[2].UInt);

}
