﻿using Dalamud.Interface.Utility.Raii;
using ImGuiNET;
using System.Diagnostics;
using System.IO;

namespace BossMod;

public sealed class AboutTab(DirectoryInfo? replayDir)
{
    private static readonly Color TitleColor = Color.FromComponents(255u, 165u, default);
    private static readonly Color SectionBgColor = Color.FromComponents(38u, 38u, 38u);
    private static readonly Color BorderColor = Color.FromComponents(178u, 178u, 178u, 204u);
    private static readonly Color DiscordColor = Color.FromComponents(88u, 101u, 242u);

    private string _lastErrorMessage = "";

    public void Draw()
    {
        using var wrap = ImRaii.TextWrapPos(0);

        ImGui.TextUnformatted("BossModReborn (BMR) provides boss fight radar, auto-rotation, cooldown planning, and AI. All of its modules can be toggled individually. Support for it can be found in the Discord server linked at the bottom of this tab.");
        ImGui.TextUnformatted("This is a FORK of the original BossMod (VBM). Only ask for support on the Combat Reborn Discord.");
        ImGui.TextUnformatted("Please also make sure to not load VBM and this fork at the same time. The consequences of doing that are unexplored and unsupported.");
        ImGui.Spacing();
        DrawSection("Radar",
        [
            "Provides an on-screen window that contains an area mini-map showing player positions, boss position(s), various imminent AOEs, and other mechanics.",
            "Useful because you don't have to remember what ability names mean.",
            "See exactly whether you're getting clipped by incoming AOEs or not.",
            "Enabled for supported bosses, visible in the \"Supported bosses\" tab.",
        ]);
        ImGui.Spacing();
        DrawSection("Autorotation",
        [
            "Executes fully optimal rotations to the best of its ability.",
            "Go to the \"Autorotation presets\" tab to create a preset.",
            "Maturity of each rotation module is present in a tooltip.",
            "Guide for using this feature can be found on the wiki.",
        ]);
        ImGui.Spacing();
        DrawSection("Cooldown planner",
        [
            "Creates a CD plan for supported bosses.",
            "Replaces autorotations in specific fights.",
            "Allows you to time specific abilities to cast at specific times.",
            "Guide for using this feature can be found on the wiki.",
        ]);
        ImGui.Spacing();
        DrawSection("AI",
        [
            "Automates movement during boss fights.",
            "Automatically moves your character based on safe zones determined by a boss's module, visible on the radar.",
            "Should not be used in when playing with unknown players.",
            "Can be hooked by other plugins to automate entire duties.",
        ]);
        ImGui.Spacing();
        DrawSection("Replays",
        [
            "Useful for creating boss modules, analyzing problems with them, and making CD plans.",
            "When asking for help, make sure to provide a replay! Please note that replays will contain your player name!",
            "Enabled in Settings > Show replay management UI (or enable auto recording).",
            $"Files are located in '{replayDir}'.",
        ]);
        ImGui.Spacing();
        ImGui.Spacing();

        using (ImRaii.PushColor(ImGuiCol.Button, DiscordColor.ABGR))
            if (ImGui.Button("Combat Reborn Discord", new(220, 0)))
                _lastErrorMessage = OpenLink("https://discord.gg/p54TZMPnC9");
        ImGui.SameLine();
        if (ImGui.Button("BossModReborn GitHub", new(220, 0)))
            _lastErrorMessage = OpenLink("https://github.com/FFXIV-CombatReborn/BossmodReborn");
        ImGui.SameLine();
        if (ImGui.Button("BossMod Wiki", new(130, 0)))
            _lastErrorMessage = OpenLink("https://github.com/awgil/ffxiv_bossmod/wiki");
        ImGui.SameLine();
        if (ImGui.Button("Open replay folder", new(180, 0)) && replayDir != null)
            _lastErrorMessage = OpenDirectory(replayDir);

        if (_lastErrorMessage.Length > 0)
        {
            using var color = ImRaii.PushColor(ImGuiCol.Text, Colors.TextColor3);
            ImGui.TextUnformatted(_lastErrorMessage);
        }
    }

    private static void DrawSection(string title, string[] bulletPoints)
    {
        using var colorBackground = ImRaii.PushColor(ImGuiCol.ChildBg, SectionBgColor.ABGR);
        using var colorBorder = ImRaii.PushColor(ImGuiCol.Border, BorderColor.ABGR);
        var height = ImGui.GetTextLineHeightWithSpacing() * (bulletPoints.Length + 2);
        using var section = ImRaii.Child(title, new(0, height), false, ImGuiWindowFlags.NoScrollbar | ImGuiWindowFlags.AlwaysUseWindowPadding);

        if (!section)
            return;

        using (ImRaii.PushColor(ImGuiCol.Text, TitleColor.ABGR))
            ImGui.TextUnformatted(title);

        ImGui.Separator();
        ImGui.PushTextWrapPos();
        foreach (var point in bulletPoints)
        {
            ImGui.Bullet();
            ImGui.SameLine();
            ImGui.TextUnformatted(point);
        }
        ImGui.PopTextWrapPos();
    }

    private static string OpenLink(string link)
    {
        try
        {
            Process.Start(new ProcessStartInfo(link) { UseShellExecute = true });
            return "";
        }
        catch (Exception e)
        {
            Service.Log($"Error opening link {link}: {e}");
            return $"Failed to open link '{link}', open it manually in the browser.";
        }
    }

    private static string OpenDirectory(DirectoryInfo dir)
    {
        if (!dir.Exists)
            return $"Directory '{dir}' not found.";

        try
        {
            Process.Start(new ProcessStartInfo(dir.FullName) { UseShellExecute = true });
            return "";
        }
        catch (Exception e)
        {
            Service.Log($"Error opening directory {dir}: {e}");
            return $"Failed to open folder '{dir}', open it manually.";
        }
    }
}
