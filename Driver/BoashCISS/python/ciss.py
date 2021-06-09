from bluepy.btle import UUID, Peripheral, ADDR_TYPE_RANDOM, DefaultDelegate
import argparse
import time
import binascii
import struct
import os
import threading

CISS_DIC = {}

CCCD_UUID = 0x2902

BATTERY_SERVICE_UUID = 0x180F
BATTERY_LEVEL_UUID = 0x2A19

USER_INTERFACE_SERVICE_UUID = 0x7500
UI_INERTIAL_CHAR_UUID       = 0x7502
UI_ENVIRONMENTAL_CHAR_UUID  = 0x7504
UI_DATA_CONTROl_CHAR_UUID   = 0x750A


ui_inertial_handle = {}
ui_environmental_handle = {}
ui_data_control_handle = {}

ui_inertial_data = {}
ui_environmental_data = {}
ui_data_control_data = {}

lock = threading.Lock()


class BatterySensor():
    svcUUID = UUID(BATTERY_SERVICE_UUID)
    dataUUID = UUID(BATTERY_LEVEL_UUID)

    def __init__(self, periph):
        self.periph = periph
        self.service = None
        self.data = None

    def enable(self):
        if self.service is None:
            self.service = self.periph.getServiceByUUID(self.svcUUID)
        if self.data is None:
            self.data = self.service.getCharacteristics(self.dataUUID)[0]

    def read(self):
        val = ord(self.data.read())
        return val

class UserInterfaceService():
    
    serviceUUID = UUID(USER_INTERFACE_SERVICE_UUID)
    inertial_char_uuid = UUID(UI_INERTIAL_CHAR_UUID)
    environmental_char_uuid = UUID(UI_ENVIRONMENTAL_CHAR_UUID)
    data_control_char_uuid = UUID(UI_DATA_CONTROl_CHAR_UUID)

    def __init__(self, periph, addr):
        self.periph = periph
        self.addr = addr
        self.ui_service = None
        self.inertial_char = None
        self.inertial_cccd = None
        self.environmental_char = None
        self.environmental_cccd = None
        self.data_control_char = None

    def enable(self):
        print ("enable")
        global ui_inertial_handle
        global ui_environmental_handle
        global ui_data_control_handle
        global ui_inertial_data
        global ui_environmental_data
        global ui_data_control_data

        if self.ui_service is None:
            self.ui_service = self.periph.getServiceByUUID(self.serviceUUID)

        lock.acquire()
        
        if self.inertial_char is None:
            self.inertial_char = self.ui_service.getCharacteristics(self.inertial_char_uuid)[0]
            ui_inertial_data[self.addr] = self.inertial_char.read()
            ui_inertial_handle[self.addr] = self.inertial_char.getHandle()
            self.inertial_cccd = self.inertial_char.getDescriptors(forUUID=CCCD_UUID)[0]

        if self.environmental_char is None:
            self.environmental_char = self.ui_service.getCharacteristics(self.environmental_char_uuid)[0]
            ui_environmental_data[self.addr] = self.environmental_char.read()
            ui_environmental_handle[self.addr] = self.environmental_char.getHandle()
            self.environmental_cccd = self.environmental_char.getDescriptors(forUUID=CCCD_UUID)[0]

        lock.release()
        
        if self.data_control_char is None:
            self.data_control_char = self.ui_service.getCharacteristics(self.data_control_char_uuid)[0]
            ui_data_control_data[self.addr] = self.data_control_char.read()
            ui_data_control_handle[self.addr] = self.data_control_char.getHandle()

        
        self.set_notification(True)
        self.data_control_char.write(b"\x19\xff\x01", False)
        
    def set_notification(self, state):
        print ("set_notification")
        if state == True:
            self.inertial_cccd.write(b"\x01\x00", False)
            self.environmental_cccd.write(b"\x01\x00", False)
        else:
            self.inertial_cccd.write(b"\x00\x00", False)
            self.environmental_cccd.write(b"\x00\x00", False)

    def set_interval(self, value):
        global ui_data_control_data
        
        self.data_control_char.write(b"\x19\xff" + struct.pack('>b', value), False)

        ui_data_control_data[self.addr] = self.data_control_char.read()

    def disable(self):
        self.set_btn_notification(False)

class MyDelegate(DefaultDelegate):

    def __init__(self, addr):
        DefaultDelegate.__init__(self)
        self.addr = addr
        
    def handleNotification(self, hnd, data):
        global ui_inertial_data
        global ui_environmental_data
        global ui_data_control_data

        lock.acquire()
        
        if (hnd == ui_inertial_handle[self.addr]):
            ui_inertial_data[self.addr] = data

        elif (hnd == ui_environmental_handle[self.addr]):
            ui_environmental_data[self.addr] = data
            
        elif (hnd == ui_data_control_handle[self.addr]):
            ui_data_control_data[self.addr] = data

        else:
            teptep = binascii.b2a_hex(data)
            print('Notification: UNKOWN: hnd {}, data {}'.format(hnd, teptep))

        lock.release()
            
class CISS(Peripheral):
    
    def __init__(self, addr):
        Peripheral.__init__(self, addr)
        Peripheral.discoverServices(self)

        self.battery = BatterySensor(self)
        self.ui = UserInterfaceService(self, addr)
        
def CISSThread(addr):
    global ui_inertial_data
    global ui_environmental_data
    global ui_data_control_data
        
    ciss = CISS(addr)
    print('Connected...')
    ciss.setDelegate(MyDelegate(addr))

    polling = {}
    polling[1] = 0.01 * 10
    polling[2] = 0.1 * 5
    polling[3] = 1 * 2
    polling[4] = 10 * 1.5
    polling[5] = 30 * 1.3
    polling[6] = 60 * 1.2
    polling[7] = 600 * 1.1
    
    try:
        ciss.ui.enable()
        
        print('All requested sensors and notifications are enabled...')
        time.sleep(1.0)

        datafile = open('/home/pi/IronServer/driver/resource/' + addr + '.txt', 'wb')
        configfile = None
        
        path = '/home/pi/IronServer/driver/resource/' + addr + '_WR.txt'

        setInterval = 1       
        errorCount = 0
        
        while True:

            try:
                configfile = open(path, 'r')
                value = int(configfile.read())
                configfile.close()
            except:
                value = 1

            if setInterval != value:
                setInterval = value
                ciss.ui.set_interval(int(setInterval))

            datafile.seek(0)
            lock.acquire()
            
            for i in range(0, 20):
                if i < len(ui_inertial_data[addr]):
                    datafile.write(struct.pack("B", ui_inertial_data[addr][i]))
                else:
                    datafile.write(struct.pack("B", 0x00))

            for i in range(0, 20):
                if i < len(ui_environmental_data[addr]):
                    datafile.write(struct.pack("B", ui_environmental_data[addr][i]))
                else:
                    datafile.write(struct.pack("B", 0x00))

            for i in range(0, 20):
                if i < len(ui_data_control_data[addr]):
                    datafile.write(struct.pack("B", ui_data_control_data[addr][i]))
                else:
                    datafile.write(struct.pack("B", 0x00))

            lock.release()
            datafile.flush()

            ciss.waitForNotifications(2)
            #if ciss.waitForNotifications(polling[setInterval]) == False:
            #    errorCount = errorCount + 1

            #    if errorCount > 5:
            #        break
            #else:
            #    errorCount = 0

        datafile.close()

    finally:
        ciss.disconnect()
        del ciss

def main():
    global CISS_DIC
    
    while True:
        f = open('/home/pi/IronServer/driver/resource/CISS.txt', 'r')

        line = f.readline()

        ciss = []
    
        while line:
            ciss.append(line[:len(line) - 1])
            line = f.readline()
                        
        f.close()

        try:
            for c in ciss:
                print ("Discover : " + c)
                t = threading.Thread(target=CISSThread, args=(c,))
                t.start()

        except:
            print("main except")

        time.sleep(10)
            
        #CISS_DIC[device.addr.upper()] = t

if __name__ == "__main__":
    main()
