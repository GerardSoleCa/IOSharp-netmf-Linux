using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Linux.SPOT.Hardware;
using System.IO;

namespace Linux.SPOT.Manager
{
    public enum PortType
    {
        NONE,
        INPUT,
        OUTPUT,
        TRISTATE,
        INTERRUPT
    };

    class GPIOManager
    {
        private static GPIOManager instance;



        private static Dictionary<Cpu.Pin, PortType> _activePins = new Dictionary<Cpu.Pin, PortType>();

        protected static String GPIO_PATH = "/sys/class/gpio/";

        private GPIOManager() { }

        public static GPIOManager Instance
        {
            get
            {
                if (instance == null)
                {
                    instance = new GPIOManager();
                }
                return instance;
            }
        }

        public void Export(Cpu.Pin pin)
        {
            if (!_activePins.ContainsKey(pin))
            {
                File.WriteAllText(GPIO_PATH + "export", ((int)pin).ToString());
                _activePins.Add(pin, PortType.NONE);
            }
        }

        //public bool IsPinExported(Cpu.Pin pin)
        //{
        //    if (_activePins.ContainsKey(pin))
        //    {
        //        return true;
        //    }
        //    else
        //    {
        //        List<String> l = Directory.GetDirectories(GPIO_PATH).ToList();
        //        if (l.Contains(GPIO_PATH + pin.ToString().Split('_')[1]))
        //        {
        //            return true;
        //        }
        //        else
        //        {
        //            return false;
        //        }
        //    }
        //}

        public void Unexport(Cpu.Pin pin)
        {
            if (_activePins.ContainsKey(pin))
            {
                File.WriteAllText(GPIO_PATH + "unexport", ((int)pin).ToString());
                _activePins.Remove(pin);
            }
        }

        public bool Read(Cpu.Pin pin)
        {
            if (_activePins.ContainsKey(pin))
            {
                //if ((_activePins[pin] == PortType.INPUT) || (_activePins[pin] == PortType.OUTPUT))
                //{
                String value = File.ReadAllText(GPIO_PATH + "gpio" + ((int)pin) + "/value");
                return value == "1" ? true : false;
                //}
                //else if ((_activePins[pin] == PortType.INTERRUPT) || (_activePins[pin] == PortType.TRISTATE))
                //{
                //    throw new NotImplementedException();
                //}
                //else
                //{
                //    throw new Exception();
                //}
            }
            else
            {
                throw new Exception();
            }
        }

        public void Write(Cpu.Pin pin, bool state)
        {
            if (_activePins.ContainsKey(pin))
            {
                if ((_activePins[pin] == PortType.OUTPUT))
                {
                    int value = state == true ? 1 : 0;
                    File.WriteAllText(GPIO_PATH + "gpio" + ((int)pin) + "/value", (value).ToString().ToLower());
                }
                else if ((_activePins[pin] == PortType.TRISTATE))
                {
                    throw new NotImplementedException();
                }
                else
                {
                    throw new Exception();
                }
            }
            else { throw new Exception(); }
        }

        public void SetPortType(Cpu.Pin pin, PortType type)
        {
            if (_activePins.ContainsKey(pin))
            {
                string direction = "";
                switch (type)
                {
                    case PortType.NONE:
                        _activePins[pin] = PortType.NONE;
                        throw new NotImplementedException();
                    case PortType.INPUT:
                        direction = "in";
                        _activePins[pin] = PortType.INPUT;
                        break;
                    case PortType.OUTPUT:
                        direction = "out";
                        _activePins[pin] = PortType.OUTPUT;
                        break;
                    case PortType.TRISTATE:
                        _activePins[pin] = PortType.TRISTATE;
                        throw new NotImplementedException();
                    case PortType.INTERRUPT:
                        _activePins[pin] = PortType.INTERRUPT;
                        throw new NotImplementedException();
                }
                File.WriteAllText(GPIO_PATH + "gpio" + ((int)pin) + "/direction", direction.ToLower().ToLower());
            }
            else
            {
                throw new Exception();
            }
        }

    }
}