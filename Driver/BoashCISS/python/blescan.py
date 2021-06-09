import argparse
import sys
from bluepy import btle
from bluepy.btle import Scanner
from ciss import *

CISS_FIND = []

class ScanPrint(btle.DefaultDelegate):

    def __init__(self, opts):
        btle.DefaultDelegate.__init__(self)
        self.opts = opts

    def handleDiscovery(self, dev, isNewDev, isNewData):
        
        if isNewDev:
            status = "new"
        elif isNewData:
            if self.opts.new:
                return
            status = "update"
        else:
            if not self.opts.all:
                return
            status = "old"

        if dev.rssi < self.opts.sensitivity:
            return

        
        for (sdid, desc, val) in dev.getScanData():
            if sdid in [8, 9]:
                if val.find("CISS") != -1 and not dev.addr.upper() in CISS_FIND:
                    CISS_FIND.append(dev.addr.upper())
                    print (desc + ': \'' + val + dev.addr + '\'')


def main():
    global CISS_FIND
    
    parser = argparse.ArgumentParser()
    parser.add_argument('-i', '--hci', action='store', type=int, default=0,
                        help='Interface number for scan')
    parser.add_argument('-t', '--timeout', action='store', type=int, default=4,
                        help='Scan delay, 0 for continuous')
    parser.add_argument('-s', '--sensitivity', action='store', type=int, default=-128,
                        help='dBm value for filtering far devices')
    parser.add_argument('-d', '--discover', action='store_true',
                        help='Connect and discover service to scanned devices')
    parser.add_argument('-a', '--all', action='store_true',
                        help='Display duplicate adv responses, by default show new + updated')
    parser.add_argument('-n', '--new', action='store_true',
                        help='Display only new adv responses, by default show new + updated')
    parser.add_argument('-v', '--verbose', action='store_true',
                        help='Increase output verbosity')
    arg = parser.parse_args(sys.argv[1:])

    btle.Debugging = arg.verbose

    scanner = btle.Scanner(arg.hci).withDelegate(ScanPrint(arg))
    

    while True:
        try:
            print ("Scanning for devices...")

            CISS_FIND = []
            devices = scanner.scan(arg.timeout)

            f = open('/home/pi/IronServer/driver/resource/CISS.txt', 'w')

            r = open('/home/pi/IronServer/driver/resource/CISS_WR.txt', 'r')

            line = r.readline()

            ciss = []
        
            while line:
                ciss.append(line[:len(line) - 1])
                line = r.readline()
                            
            r.close()
        
            for device in devices:
                if device.addr.upper() in CISS_FIND and device.connectable:
                    if device.addr.upper() in ciss:
                        print ("Ready : " + device.addr)
                        f.write(device.addr.upper() + "\n")

            f.close()

            time.sleep(1)

        except:
            continue

if __name__ == "__main__":
    main()
