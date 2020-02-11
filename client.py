from   tensorflow.keras.models         import Sequential
from   tensorflow.keras.layers         import Conv2D, Dense, Flatten
from   tensorflow.keras.optimizers     import RMSprop, Adam

import numpy                as np
import matplotlib.pyplot    as plt
import matplotlib.animation as animation

import gtenv                as gt
from   zoopy                import *

import os
os.environ['CUDA_VISIBLE_DEVICES'] = '-1'

import tensorflow as tf

print(tf.compat.v1.test.is_gpu_available())
	
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

class ZGTEnvironment(Environment):
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
		return State(state=self._frames)

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

		res        = Result()
		res.state  = State(state=self._frames)
		res.reward = [reward]
		res.done   = done
		res.info   = None
		return res

	def render(self, episode=None, step=None):
		self._env.render()


def keras_model_build(parameters):
	model = Sequential()
	model.add(Conv2D(8, kernel_size=3, strides=2, activation='relu', input_shape=parameters['state_size']))
	model.add(Conv2D(8, kernel_size=3, strides=2, activation='relu'))
	model.add(Conv2D(8, kernel_size=3, strides=2, activation='relu'))
	model.add(Flatten())
	model.add(Dense(parameters['action_size'], activation='softmax'))

	learning_rate = 0.001
	#opt  = RMSprop(lr=learning_rate)
	opt  = Adam(lr=learning_rate)
	loss = 'mse'
	model.compile(loss=loss, optimizer=opt, metrics=['mae'])
	return model


env = ZGTEnvironment(address='146.48.87.139', port=8086)
parameters = {
	'state_size'  : env.state_size  (),
	'action_size' : env.action_size ()
}

agent = keras_dqn_agent(keras_model_build, parameters=parameters)

'''
episodes  = []
rewards   = []
means     = []
acc_mean  = 0.0
acc_count = 0

fig = plt.figure()
ax1 = fig.add_subplot(1, 1, 1)
plt.xlabel('episode')
plt.ylabel('reward')

def animate(frame):
	ax1.clear()
	ax1.plot(episodes, rewards, color='orange')
	ax1.plot(episodes, means  , color='blue'  )

class Callbacks(zp.EventListener):
	def episode_end(self, episode=None):
		global episodes
		global rewards
		global means
		global acc_mean
		global acc_count

		count = 10
		if acc_count > count:
			acc_mean -= rewards[-(count+1)]
		else:
			acc_count += 1

		episodes .append(episode)
		rewards  .append(agent.episode_cumulative_reward())

		acc_mean += rewards[-1]
		means.append(acc_mean / acc_count)

ani = animation.FuncAnimation(fig, animate, interval=1000)
plt.show(block=False)

zp.simulate(environment=env, agents=[agent], episodes=1, listeners=[Callbacks()], disable_render=False, plt=plt)
'''

simulate(environment=env, agents=[agent], episodes=1, disable_render=False)



'''
for i in range(1):
	result = env.reset()
	done   = result['Data']['Terminated']

	while not done:
		env.render()

		image  = result['Data']['NextState']['Values'][0]['Image']
		frame  = np.asarray(image, dtype=np.uint8).astype(np.float32) * (2.0 / 255.0) - 1.0
		frames = np.concatenate((frames[3:, :, :], frame), axis=0)
		action = compute_action(frames)
		result = env.step(action)
		done   = result['Data']['Terminated']

env.close()
'''
