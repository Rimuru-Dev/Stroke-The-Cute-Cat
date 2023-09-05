// ReSharper disable CommentTypo
// **************************************************************** //
//
//   Copyright (c) RimuruDev. All rights reserved.
//   Contact me: 
//          - Gmail:    rimuru.dev@gmail.com
//          - GitHub:   https://github.com/RimuruDev
//          - LinkedIn: https://www.linkedin.com/in/rimuru/
//
// **************************************************************** //

using System;

namespace RimuruDev.Internal.Codebaase.Rintime.Background
{
    [Serializable]
    public enum LoopMode
    {
        Update = 0,
        FixedUpdate = 1,
        LateUpdate = 2,
    }
}