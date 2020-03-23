# coding: utf-8
# noinspection PyPackageRequirements
from sqlalchemy import CHAR, Column, String, text
# noinspection PyPackageRequirements
from sqlalchemy.dialects.mysql import INTEGER
# noinspection PyPackageRequirements
from sqlalchemy.ext.declarative import declarative_base

Base = declarative_base()
metadata = Base.metadata


class Radreply(Base):
    def __init__(self):
        pass

    __tablename__ = 'radreply'

    id = Column(INTEGER(11), primary_key=True)
    username = Column(String(64), nullable=False, index=True, server_default=text("''"))
    attribute = Column(String(64), nullable=False, server_default=text("''"))
    op = Column(CHAR(2), nullable=False, server_default=text("'='"))
    value = Column(String(253), nullable=False, server_default=text("''"))
