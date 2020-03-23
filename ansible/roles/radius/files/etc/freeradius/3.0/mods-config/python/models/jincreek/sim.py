# coding: utf-8
# noinspection PyPackageRequirements
from sqlalchemy import Column, ForeignKey, Integer, String
from sqlalchemy.orm import relationship
# noinspection PyPackageRequirements
from sqlalchemy.dialects.mysql import DATETIME, INTEGER, SMALLINT, TINYINT
# noinspection PyPackageRequirements
from sqlalchemy.ext.declarative import declarative_base

Base = declarative_base()
metadata = Base.metadata


class Sim(Base):
    def __init__(self):
        pass

    __tablename__ = 'Sim'

    Id = Column(String(36), primary_key=True)
    Msisdn = Column(String(15), nullable=False)
    Imsi = Column(String(32), nullable=False)
    IccId = Column(String(19), nullable=False)
    UserName = Column(String(64), nullable=False)
    Password = Column(String(64), nullable=False)
    SimGroupId = Column(String(36), ForeignKey('SimGroup.Id'), nullable=True)

class SimGroup(Base):
    def __init__(self):
        pass

    __tablename__ = 'SimGroup'

    Id = Column(String(36), primary_key=True)
    OrganizationCode = Column(Integer, nullable=False)
    Name = Column(String(256), nullable=False)
    Apn = Column(String(256), nullable=False)
    NasIp = Column(String(43), nullable=False)
    IsolatedNw1IpPool = Column(String(30), nullable=False)
    IsolatedNw1IpRange = Column(String(43), nullable=False)
    AuthenticationServerIp = Column(String(43), nullable=False)
    PrimaryDns = Column(String(43), nullable=False)
    SecondaryDns = Column(String(43), nullable=False)
    IsolatedNw1PrimaryDns = Column(String(43), nullable=False)
    IsolatedNw1SecondaryDns = Column(String(43), nullable=False)
    UserNameSuffix = Column(String(64), nullable=False)
    Sims = relationship('Sim', backref='SimGroup')

