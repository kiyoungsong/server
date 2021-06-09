from System.Collections.Generic import *
from System.Collections import BitArray
import binascii

isSwap = false

##initSize
def InitSize():
    result = {  
                "readMaxSize" : 8191,          ##Data Read Max Size (0x1fff)
                "writeMaxSize" : 8191,           ##Data Write Max Size (0x1fff) 
                "headerLength" : 20,          ##Command Header Length (i think driver doesn't need)
                "bodyLength" : 0,             ##Command body Length     
                "readCommandLength" : 20,     ##Command Read Total Length
                "writeCommandLength" : 22,    ##Comannd Write Total Length
                "resHeader" : 0 }              ##Reponse Read/Write Command Length 
    return Dictionary[str, int](result)


def InitValue():
    ##Device Values m,p, ....
    devices = { 'x' : "\x9C", 'y' : "\x9D", 'm' : "\x90", 'w' : "\xB4", 'l' : "\x92", 'f' : "\x93",
                'v' : "\x94", 'b' : "\xA0", 'd' : "\xA8", 'ts' : "\xC1", 'tc' : "\xC0", 'tn' : "\xC2",
                'cs' : "\xC4", 'cc' : "\xC3", 'cn' : "\xC5", 'sb' : "\xA1", 'sw' : "\xB5", 'dx' : "\xA2",
                'dy' : "\xA3", 'sm' : "\x91", 'sd' : "\xA9" }  
    
    return Dictionary[str, str](devices)


##Packet
def MakePacket(byCMD, device, dwRegNum, dwStartAddr, offset, writeBuff):
    packet = ""

    packet += "\x50"
    packet += "\x00"
    
    #QHeader
    packet += "\x00"
    packet += "\xff"
    packet += "\xff"
    packet += "\x03"
    packet += "\x00"
    
    #if byCMD == 1:
    #    packet += "\x0c" ##data length L //read
    #    packet += "\x00" ##data length H
    #else:
    #    packet += "\x0e" ##data length L //write
    #    packet += "\x00" ##data length H

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
    packet += dwStartAddr[:1] ## Address = 0
    packet += dwStartAddr[1:2]
    packet += "\x00"
    
    #Device Type
    packet += device
        
    #Device Length
    packet += dwRegNum[:1]
    packet += dwRegNum[1:2]
    
    #writeCommand
    if byCMD == 2:
        for i in range(len(writeBuff)):
            packet += writeBuff[i:i+1]
        
        lengthPacket = ""   ##data length Packet
        if len(packet[7:])%2 == 1:
            legnth = hex(len(packet[7:])).replace("0x","")
            lengthPacket += binascii.a2b_hex('0'+legnth)    ##data length L Packet
        else:
            legnth = hex(len(packet[7:])).replace("0x","")
            lengthPacket += binascii.a2b_hex(legnth)        ##data length L Packet
        
        if len(lengthPacket) == 1:
            lengthPacket += "\x00"                          ##data length H Packet

        packet = packet[:7] + lengthPacket + packet[7:]     ##Write Request Packet
    else:
        packet = packet[:7] + "\x0c\x00" + packet[7:]       ##Read Request Packet

    return packet

def ExportData(packet, isBit):
    
    tempPacket = packet[17:]
    
    if isSwap:
        tempList = list(packet)

        for i in range(0, len(tempList), 2):    #Swap
            tempList[i], tempList[i + 1] = tempList[i + 1], tempList[i]
        packet = tempList            

    if isBit:
        tempPacket = ''.join(format(ord(x), 'b') for x in tempPacket)
    
    return tempPacket

def SetMaxSize(packet, a):

    print (packet)

    return "data"