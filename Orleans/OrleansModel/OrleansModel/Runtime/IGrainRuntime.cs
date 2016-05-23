﻿//-----------------------------------------------------------------------
// <copyright file="IGrainRuntime.cs">
//      Copyright (c) Microsoft Corporation. All rights reserved.
// 
//      THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND,
//      EXPRESS OR IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF
//      MERCHANTABILITY, FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT.
//      IN NO EVENT SHALL THE AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY
//      CLAIM, DAMAGES OR OTHER LIABILITY, WHETHER IN AN ACTION OF CONTRACT,
//      TORT OR OTHERWISE, ARISING FROM, OUT OF OR IN CONNECTION WITH THE
//      SOFTWARE OR THE USE OR OTHER DEALINGS IN THE SOFTWARE.
// </copyright>
//-----------------------------------------------------------------------

using System;

namespace Orleans.Runtime
{
    /// <summary>
    /// Interface of the Orleans grain runtime.
    /// </summary>
    public interface IGrainRuntime
    {
        /// <summary>
        /// The grain factory.
        /// </summary>
        IGrainFactory GrainFactory { get; }

        /// <summary>
        /// The service provider.
        /// </summary>
        IServiceProvider ServiceProvider { get; }
    }
}