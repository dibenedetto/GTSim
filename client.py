import base64
import gtenv

'''
def save(result, step):
	encoded = result['Data']['NextState']['Values'][0]['Image'][0]
	decoded = base64.b64decode(encoded)
	f = open(str(step) + '.jpg', 'wb')
	f.write(decoded)
	f.close()
'''

env = gtenv.GTEnvironment()

for i in range(1):
	result = env.reset()
	step   = 0
	done   = result['Data']['Terminated']

	while not done:
		env.render()

		step   += 1
		action  = { 'Values': [ { 'Data': [ 40.0 ] }, None, None ] }
		result  = env.step(action)
		done    = result['Data']['Terminated']

env.close()
