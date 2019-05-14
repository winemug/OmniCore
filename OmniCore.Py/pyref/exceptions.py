
class OmnipyError(Exception):
    def __init__(self, message="Unknown"):
        self.error_message = message


class PacketRadioError(OmnipyError):
    def __init__(self, message="Unknown RL error", err_code=None):
        OmnipyError.__init__(self, message)
        self.err_code = err_code


class ProtocolError(OmnipyError):
    def __init__(self, message="Unknown protocol error"):
        OmnipyError.__init__(self, message)

class OmnipyTimeoutError(OmnipyError):
    def __init__(self, message="Timeout error"):
        OmnipyError.__init__(self, message)

class PdmError(OmnipyError):
    def __init__(self, message="Unknown pdm error"):
        OmnipyError.__init__(self, message)


class PdmBusyError(PdmError):
    def __init__(self, message="Pdm is busy."):
        PdmError.__init__(self, message)
