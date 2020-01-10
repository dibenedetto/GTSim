import socket
import json

GTSIM_ADDRESS     = '127.0.0.1'
GTSIM_PORT        = 8086
GTSIM_BUFFER_SIZE = 8 * 1024 * 1024

s = socket.socket(socket.AF_INET, socket.SOCK_STREAM)
s.connect((GTSIM_ADDRESS, GTSIM_PORT))

message = b'{"Code":"Explain", "Data":null}'
s.send(message)
print('sent: ' + message.decode('utf-8'))

running = True
while running:
	data   = s.recv(GTSIM_BUFFER_SIZE)
	str    = data.decode('utf-8')
	print('recv: ' + str)
	result = json.loads(str)

	code    = result['Code']
	message = None

	if code == 'Ping':
		message = b'{"Code":"Pong", "Data":null}'
	elif code == 'Explain':
		message = b'{"Code":"Reset", "Data":null}'
	elif code == 'Reset':
		running = not result['Data']['Terminated']
		message = b'{"Code":"Step", "Data":{"Values":[{"Data":[40.0]}, null, null]}}'
	elif code == 'Step':
		running = not result['Data']['Terminated']
		message = b'{"Code":"Step", "Data":{"Values":[{"Data":[40.0]}, null, null]}}'
	else:
		pass

	s.send(message)
	print('sent: ' + message.decode('utf-8'))


'''
for i in range(1):
	message = b'{"Code":"Reset"}'
	s.send(message)
	data = s.recv(GTSIM_BUFFER_SIZE)
	str  = data.decode('utf-8')
	print('Reset: ' + str)

	result = json.loads(str)

	message = b'{"Code":"Step", "Action":{"Values":[{"Data":[40.0]}, null, null]}}'

	running = True
	while running:
		s.send(message)
		data = s.recv(GTSIM_BUFFER_SIZE)
		str  = data.decode('utf-8')
		print('Step: ' + str)

		result  = json.loads(str)
		running = not result['Data']['Terminated']
'''

message = b'{"Code":"Quit", "Data":null}'
s.send(message)

s.close()
