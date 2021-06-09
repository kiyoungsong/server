from System.Collections.Generic import *
import binascii

isSwap = False

class Melsec:
    ##initSize
    def InitSize(self):
        result = {  
                    "readMaxSize" : 8191,          ##Data Read Max Size (0x1fff)
                    "writeMaxSize" : 8191,           ##Data Write Max Size (0x1fff) 
                    "headerLength" : 20,          ##Command Header Length (i think driver doesn't need)
                    "bodyLength" : 0,             ##Command body Length     
                    "readCommandLength" : 11,     ##Command Read Total Length
                    "writeCommandLength" : 22,    ##Comannd Write Total Length
                    "resHeader" : 0,              ##Reponse Read/Write Command Length 
                    "isBit" : 0 }                 ##Bit Address not Byte Address : 1 / Bit Address equal byte Address : 0
        return Dictionary[str, int](result)
    
    
    def InitValue(self):
        ##Device Values m,p, ....
        devices = { 'x' : "\x9C", 'y' : "\x9D", 'm' : "\x90", 'w' : "\xB4", 'l' : "\x92", 'f' : "\x93",
                    'v' : "\x94", 'b' : "\xA0", 'd' : "\xA8", 'ts' : "\xC1", 'tc' : "\xC0", 'tn' : "\xC2",
                    'cs' : "\xC4", 'cc' : "\xC3", 'cn' : "\xC5", 'sb' : "\xA1", 'sw' : "\xB5", 'dx' : "\xA2",
                    'dy' : "\xA3", 'sm' : "\x91", 'sd' : "\xA9" }
    
        return Dictionary[str, str](devices)
    
    ##Bit/Word distinguish
    def BitWordDistinguish(self, deviceType):
        bitDevices = [ 'x', 'y', 'm', 'l', 'f', 'v', 'b', 'ts', 'tc', 'cs', 'cc', 'sb', 'dx', 'dy', 'sm' ]
        result = False
    
        if deviceType in bitDevices:
            result = True
        else:
            result = False
    
        return result
    
    ##Packet
    def MakePacket(self, byCMD, device, dwRegNum, dwStartAddr, offset, writeBuff):
        packet = ""
    
        packet += "\x50"
        packet += "\x00"
        
        #QHeader
        packet += "\x00"
        packet += "\xff"
        packet += "\xff"
        packet += "\x03"
        packet += "\x00"
        
        packet += "\x10"
        packet += "\x00"
        
        #Command
        if byCMD == 1:
            packet += "\x01" ## byCMD == 1 : Read
            packet += "\x04"
        else:
            packet += "\x01" ## byCMD == 2 : Write
            packet += "\x14"
        
        #SubCommand
        packet += "\x00"
        packet += "\x00"
        
        #Device Address
        hexNum = hex(dwStartAddr)
        temp = hexNum.replace("0x","")
        if (len(hexNum)-3)/2 == 0:
            temp += "00"
            
        if len(temp)%2 == 1:
            temp = "0" + temp
            hexAddr = binascii.a2b_hex(temp)
        else:
            hexAddr = binascii.a2b_hex(temp)
    
        packet += hexAddr[:1] ## Address = 0
        packet += hexAddr[1:2]
        packet += "\x00"
        
        #Device Type
        hexNum = hex(device)
        deviceType = hexNum.replace("0x","")
        if len(hexNum)%2 == 1:
            packet += binascii.a2b_hex('0'+deviceType)
        else:
            packet += binascii.a2b_hex(deviceType)
        
        #Device Length
        hexNum = hex(dwRegNum)
        temp = hexNum.replace("0x","")
        
        if (len(hexNum)-3)/2 == 0:
            temp += "00"
        
        if len(temp)%2 == 1:
            temp = "0" + temp
            hexAddr = binascii.a2b_hex(temp)
        else:
            hexAddr = binascii.a2b_hex(temp)
    
        packet += hexAddr[:1]
        packet += hexAddr[1:2]
        
        #writeCommand
        if byCMD == 2:
            write = bytearray(writeBuff)
            writePacket = ""
    
            if len(write)%2 == 1:
                write += "\x00"
            
            temp = write.decode('utf-16')
            tempByte = temp.encode('utf-16')
    
            for i in range(2, len(tempByte), 1):
                writePacket += tempByte[i:i+1]
    
            packet += writePacket
            
            ##data length Packet
            lengthPacket = ""
            lengthTemp = hex(len(packet[7:])).replace("0x","")
                
            if len(lengthTemp)%2 == 1:
                lengthTemp = "0" + lengthTemp
    
            lengthPacket += binascii.a2b_hex(lengthTemp)        ##data length L Packet
            
            if len(lengthPacket) == 1:
                lengthPacket += "\x00"                          ##data length H Packet
    
            packet = packet[:7] + lengthPacket + packet[7:]     ##Write Request Packet
        else:
            packet = packet[:7] + "\x0c\x00" + packet[7:]       ##Read Request Packet

        return packet
    
    def ExportData(self, packet, dwRegNum):
        
        tempByte = bytearray(packet)
        byteArray = tempByte[11:]
        if len(byteArray)%2 == 1:
            byteArray.extend('\x00')
        
        global isSwap
        if isSwap:
            for i in range(0, len(byteArray), 2):    #Swap
                byteArray[i], byteArray[i + 1] = byteArray[i + 1], byteArray[i]
        
        decodeArray = byteArray.decode('utf-16')
        tempPacket = decodeArray[:dwRegNum]
    
        return tempPacket