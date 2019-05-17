#! /usr/local/bin/python3
import zmq
import sched,time
from  datetime import datetime
import random
import numpy as np
def do_PCA(pos):
	print("PCA's")
	time.sleep(5)
	print(pos)
context = zmq.Context()
socket = context.socket(zmq.REQ)
socket.connect("tcp://localhost:12346")

TIMEOUT = 10000


pos = np.array([])
s = datetime.now()
while True:
	socket.send_string("request")
	poller = zmq.Poller()
	poller.register(socket, zmq.POLLIN)
	evt = dict(poller.poll(TIMEOUT))
	if evt:
		if evt.get(socket) == zmq.POLLIN:
			response = socket.recv(zmq.NOBLOCK)
			print(response)
			#print(pos)
			pos = np.append(pos,response)
			#print(pos)
			if (datetime.now() - s).total_seconds() >= 3:
				do_PCA(pos)
				pos = np.array([])
				s = datetime.now()
			continue
	time.sleep(0.5)
	socket.close()
	socket = context.socket(zmq.REQ)
	socket.connect("tcp://localhost:12346")

