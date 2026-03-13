//-----------------------------------------------------------------------
// <copyright file="MessageBubbleComponent.razor.cs" company="Luppes Consulting, Inc.">
// Copyright 2026, Luppes Consulting, Inc. All rights reserved.
// </copyright>
// <summary>
// Message bubble component code-behind
// </summary>
//-----------------------------------------------------------------------
namespace DadABase.Web.Components;

/// <summary>
/// Displays one chat message bubble.
/// </summary>
public partial class MessageBubbleComponent : ComponentBase
{
    /// <summary>
    /// Gets or sets the message content and metadata.
    /// </summary>
    [Parameter]
    public MessageBubble message { get; set; }
}