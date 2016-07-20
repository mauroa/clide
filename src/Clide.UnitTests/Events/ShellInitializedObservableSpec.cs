﻿using System;
using System.Threading.Tasks;
using Microsoft.VisualStudio;
using Microsoft.VisualStudio.Shell.Interop;
using Moq;
using Xunit;
using System.Reactive.Linq;
using System.Threading;

namespace Clide.Events
{
	public class ShellInitializedObservableSpec
	{
		const int ZombieProperty = (int)__VSSPROPID.VSSPROPID_Zombie;

		[Fact]
		public void when_subscribing_to_initialized_shell_then_receives_event_and_completes()
		{
			object zombie = false;
			var observable = new ShellInitializedObservable(Mock.Of<IVsShell>(shell => shell.GetProperty(ZombieProperty, out zombie) == VSConstants.S_OK));

			var completed = false;
			ShellInitialized data = null;

			using (observable.Subscribe(e => data = e, () => completed = true)) { }

			Assert.True(completed);
			Assert.NotNull(data);
		}

		[Fact]
		public async Task when_subscribing_to_noninitialized_shell_then_can_wait_event_and_completion()
		{
			object zombie = true;
			uint cookie = 1;
			IVsShellPropertyEvents callback = null;

			var shell = new Mock<IVsShell>();
			shell.Setup(x => x.GetProperty(ZombieProperty, out zombie)).Returns(VSConstants.S_OK);

			var capture = new CaptureMatch<IVsShellPropertyEvents>(s => callback = s);
			shell.Setup(x => x.AdviseShellPropertyChanges(Capture.With(capture), out cookie))
				.Returns(VSConstants.S_OK);

			var observable = new ShellInitializedObservable(shell.Object);

			// Callback should have been provided at this point.
			Assert.NotNull(callback);

			var completed = false;
			ShellInitialized data = null;

			observable.Subscribe(e => data = e, () => completed = true);

			Assert.False(completed);
			Assert.Null(data);

			callback.OnShellPropertyChange(ZombieProperty, false);

			SpinWait.SpinUntil(() => completed, 200);

			Assert.True(completed);
			Assert.NotNull(data);

			shell.Verify(x => x.UnadviseShellPropertyChanges(cookie));

			// Subsequent subscription should get one and complete right away.

			var ev = await observable;

			Assert.Same(data, ev);
		}
	}
}