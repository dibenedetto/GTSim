import numpy                as np
import matplotlib.pyplot    as plt
import matplotlib.animation as animation

import gtenv                as gt
import zoopy                as zp

'''
actions:
	0 = "speed-keep",
	1 = "speed-accelerate",
	2 = "speed-brake",
	3 = "steer-center",
	4 = "steer-left",
	5 = "steer-right",

mapping:
	0 = "speed-keep"       + "steer-center" [0-3]
	1 = "speed-keep"       + "steer-left"   [0-4]
	2 = "speed-keep"       + "steer-right"  [0-5]
	3 = "speed-accelerate" + "steer-center" [1-3]
	4 = "speed-accelerate" + "steer-left"   [1-4]
	5 = "speed-accelerate" + "steer-right"  [1-5]
	6 = "speed-brake"      + "steer-center" [2-3]
	7 = "speed-brake"      + "steer-left"   [2-4]
	8 = "speed-brake"      + "steer-right"  [2-5]

'''

class ZGTEnvironment(zp.Environment):
	def __init__(self, address='127.0.0.1', port=8086):
		super(ZGTEnvironment, self).__init__()

		env = gt.GTEnvironment(address, port)

		self._env = env

		FRAME_SLOT = 4
		self._state_size  = (env.frame_height(), env.frame_width(), FRAME_SLOT * env.frame_channels())
		self._action_size = 9
		self._frames      = np.zeros(self._state_size)

		self._table = [
			(0, 3), (0, 4), (0, 5),
			(1, 3), (1, 4), (1, 5),
			(2, 3), (2, 4), (2, 5)
		]

	def _add_frame(self, result):
		image = result['Data']['NextState']['Values'][0]['Image'][0]
		frame = np.array(image, dtype=np.uint8).astype(np.float32) * (2.0 / 255.0) - 1.0
		self._frames = np.concatenate((self._frames[:, :, 3:], frame), axis=2)

	def state_size(self):
		return self._state_size

	def action_size(self):
		return self._action_size

	def reset(self, episode=None):
		result = self._env.reset()
		self._add_frame(result)
		return zp.State(state=self._frames)

	def step(self, episode=None, step=None, actions=None):
		apply  = { 'Data': [ 0.0 ] }
		values = [ None, None, None, None, None, None ]
		action = { 'Values': values }

		speed, steer  = self._table[actions[0]]

		values[speed] = apply
		values[steer] = apply

		result = self._env.step(action)
		self._add_frame(result)

		reward = result['Data']['Reward'    ]
		done   = result['Data']['Terminated']

		res        = zp.Result()
		res.state  = zp.State(state=self._frames)
		res.reward = [reward]
		res.done   = done
		res.info   = None
		return res

	def render(self, episode=None, step=None):
		return self._env.render()

	def close(self):
		return self._env.close()

exe = { 'Data': [ 1.0 ] }
nop = None

env = ZGTEnvironment(address='127.0.0.1', port=8086)

'''
for i in range(1):
	result = env.reset()
	done   = result['Data']['Terminated']

	action = {
		'Values': [
			nop,
			nop,
			exe,
			nop,
			nop,
			nop,
			nop,
			nop,
			nop
		]
	}

	while not done:
		env.render()
		result = env.step(action)
		done   = result['Data']['Terminated']
'''
result = env.reset()

env.close()
