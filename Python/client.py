#! /usr/local/bin/python3
import zmq
import time
from  datetime import datetime
import numpy as np
from sklearn.decomposition import PCA
time.sleep(2) # buffer for 2 seconds [wait to start the game]

s = datetime.now()

#--------------- send PCA back to Unity

context1 = zmq.Context()
socket1 = context1.socket(zmq.PUB)
socket1.bind("tcp://*:12345")


def do_PCA(pos,ct):
	print("Calculating PCA")
	data = pos[:ct,:]
	ipca = PCA(n_components=3, svd_solver='full') #arpack, full
	ipca.fit(data) # ipca.transform(data)
	message = ipca.components_
	print(message)
	socket1.send_string(str(message))# can send str or unicode
	#print(ipca.get_covariance(data))
	return None

context = zmq.Context()
socket = context.socket(zmq.REQ)
socket.connect("tcp://localhost:12346")

TIMEOUT = 10000

n = 5 # total second amount for data block
ct = 0 # index  for pos
pos = np.zeros([n*3000,3])


while True:
	socket.send_string("request")
	poller = zmq.Poller()
	poller.register(socket, zmq.POLLIN)
	evt = dict(poller.poll(TIMEOUT))
	if evt:
		if evt.get(socket) == zmq.POLLIN:
			response = socket.recv(zmq.NOBLOCK)
			#print(response)

			chars = response.decode("utf-8")
			pos[ct] = list(map(float,chars.split(" ")))
			ct += 1

			if (datetime.now() - s).total_seconds() >= n:
				do_PCA(pos,ct)
				s = datetime.now()
				pos = np.zeros([n * 3000, 3])
				ct = 0
			continue
	time.sleep(0.5)
	socket.close()
	socket = context.socket(zmq.REQ)
	socket.connect("tcp://localhost:12346")

