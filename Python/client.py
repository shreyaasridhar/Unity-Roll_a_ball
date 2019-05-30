#! /usr/local/bin/python3
import zmq
import time
from  datetime import datetime
import numpy as np
from sklearn.decomposition import PCA
n = 5 # total second amount for data block
s = datetime.now()

#---- Sending PCA back to Unity ----------
context1 = zmq.Context()
socket1 = context1.socket(zmq.PUB)
socket1.bind("tcp://*:12345")

def do_PCA(pos,ct):
	print("Calculating PCA")
	data = pos[:ct,:]

	# Compute PCA
	ipca = PCA(n_components=3, svd_solver='full')
	ipca.fit(data) # ipca.transform(data)
	PCA_vectors = ipca.components_
	#print('PCA_vectors', PCA_vectors)

	# Compute magnitude
	eigenvalues = ipca.explained_variance_

	# Compute centroid
	centroid = ipca.mean_

	end_points = []
	for length, vector in zip(eigenvalues, PCA_vectors):
		v = vector * 3 * np.sqrt(length)
		end_points.append(v + centroid)
	message = str(centroid) + "\n"+str(end_points[0])+"\n"+str(end_points[1])+"\n"+str(end_points[2])
	socket1.send_string(message)
	print(message)
	return None

context = zmq.Context()
socket = context.socket(zmq.REQ)
socket.connect("tcp://localhost:12346")

#---- GETTING DATA FROM Unity ----------
TIMEOUT = 10000

ct = 0 # index  for pos
pos = np.zeros([n*3000,3])

print("Instantiated")
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
				# if there isn't enough data in CT, don't jump into this if
				if ct>3:
					do_PCA(pos,ct)
					s = datetime.now()
					pos = np.zeros([n * 3000, 3])
					ct = 0
				else:
					print("Not enough data nomnom")

			continue
	time.sleep(0.5)
	socket.close()
	socket = context.socket(zmq.REQ)
	socket.connect("tcp://localhost:12346")